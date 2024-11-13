using Telegram.Bot.Types;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using ClosedXML.Excel;

public static class DatabaseManager
{
    // Добавляет пользователя в БД
    public static void AddUser(Telegram.Bot.Types.User user)
    {
        using (var context = new ShopDbContext())
        {
            // Получаем роль пользователя
            var userRole = context.Roles.FirstOrDefault(r => r.RoleName == "Пользователь");

            // Проверяем, существует ли пользователь с таким UserId

            if (!context.Users.Any(u => u.UserId == user.Id))
            {
                // Добавляем нового пользователя
                context.Users.Add(new User
                {
                    UserId = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Username = user.Username,
                    LanguageCode = user.LanguageCode,
                    RoleId = userRole.RoleId,
                    LastActivityDate = DateTime.UtcNow
                });
                context.SaveChanges();
            }

        }
    }

    // Добавляет администратора в БД
    public static void AddUserAdmin(Telegram.Bot.Types.User user)
    {
        using (var context = new ShopDbContext())
        {// Получаем роль пользователя
            var userRole = context.Roles.FirstOrDefault(r => r.RoleName == "Администратор");

            // Проверяем, существует ли пользователь с таким UserId

            if (!context.Users.Any(u => u.UserId == user.Id))
            {
                // Добавляем нового пользователя
                context.Users.Add(new User
                {
                    UserId = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Username = user.Username,
                    LanguageCode = user.LanguageCode,
                    RoleId = userRole.RoleId,
                    RegistrationDate = DateTime.UtcNow
                });
                context.SaveChanges();
            }
        }
    }

    // Добавляет продавца в БД
    public static void AddUserWorker(Telegram.Bot.Types.User user)
    {
        using (var context = new ShopDbContext())
        {// Получаем роль пользователя
            var userRole = context.Roles.FirstOrDefault(r => r.RoleName == "Продавец");

            // Проверяем, существует ли пользователь с таким UserId

            if (!context.Users.Any(u => u.UserId == user.Id))
            {
                // Добавляем нового пользователя
                context.Users.Add(new User
                {
                    UserId = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Username = user.Username,
                    LanguageCode = user.LanguageCode,
                    RoleId = userRole.RoleId,
                    RegistrationDate = DateTime.UtcNow
                });
                context.SaveChanges();
            }
        }
    }

    // Заполняет Роли пользователей 
    public static void AddRoles()
    {
        using (var context = new ShopDbContext())
        {
            // Проверка наличия ролей
            if (!context.Roles.Any())
            {
                // Добавление ролей
                context.Roles.AddRange(
                    new Role { RoleName = "Администратор", Description = "Полный доступ к системе" },
                    new Role { RoleName = "Продавец", Description = "Обработка заказов и поддержка клиентов" },
                    new Role { RoleName = "Пользователь", Description = "Обычный пользователь" }
                );
                context.SaveChanges();
            }
        }
    }

    // читает данные из Excel-файла и записивает список категорий и продуктов в БД:
    public static async Task ImportProductsFromExcel(string filePath, TelegramBotClient bot, Message msg)
    {
        using var workbook = new XLWorkbook(filePath);
        using var context = new ShopDbContext();
        using var transaction = context.Database.BeginTransaction();

        try
        {
            // Чтение данных из Excel
            var worksheet1 = workbook.Worksheet(1); // Получаем первый лист в книге
            var rowsList1 = worksheet1.RowsUsed().Skip(1); // Пропускаем первую строку с заголовками
            
            var worksheet2 = workbook.Worksheet(2); // Получаем второй лист в книге
            var rowsList2 = worksheet2.RowsUsed().Skip(1); // Пропускаем первую строку с заголовками

            // --- Синхронизация категорий ---
            var categoriesFromExcel = new List<Category>();
            var categoryNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var row in rowsList2)
            {
                var categoryName = row.Cell(1).GetString().Trim();

                if (!string.IsNullOrWhiteSpace(categoryName) && categoryNames.Add(categoryName))
                {
                    var category = new Category
                    {
                        CategoryName = categoryName,
                        Description = row.Cell(2).GetString()
                    };

                    categoriesFromExcel.Add(category);
                }
            }

            // Получаем существующие категории из базы данных
            var existingCategories = context.Categories.ToList();

            // Создаём словари для быстрого доступа
            var categoryNamesFromFile = new HashSet<string>(categoriesFromExcel.Select(c => c.CategoryName), StringComparer.OrdinalIgnoreCase);
            var existingCategoryNames = new HashSet<string>(existingCategories.Select(c => c.CategoryName), StringComparer.OrdinalIgnoreCase);

            // Категории для добавления
            var categoriesToAdd = categoriesFromExcel.Where(c => !existingCategoryNames.Contains(c.CategoryName)).ToList();

            // Категории для удаления
            var categoriesToDelete = existingCategories.Where(c => !categoryNamesFromFile.Contains(c.CategoryName)).ToList();

            // Добавляем новые категории
            if (categoriesToAdd.Any())
            {
                context.Categories.AddRange(categoriesToAdd);
                await context.SaveChangesAsync();
            }

            // Удаляем категории, которых нет в файле
            if (categoriesToDelete.Any())
            {
                context.Categories.RemoveRange(categoriesToDelete);
                await context.SaveChangesAsync();
            }

            // Обновляем существующие категории
            foreach (var category in existingCategories)
            {
                var categoryFromFile = categoriesFromExcel.FirstOrDefault(c => c.CategoryName.Equals(category.CategoryName, StringComparison.OrdinalIgnoreCase));
                if (categoryFromFile != null)
                {
                    category.Description = categoryFromFile.Description;
                    // Обновите другие свойства при необходимости
                }
            }

            await context.SaveChangesAsync();

            // Создаём словарь категорий для быстрого доступа по имени
            var categoryDict = context.Categories.ToDictionary(c => c.CategoryName, c => c.CategoryId, StringComparer.OrdinalIgnoreCase);

            // --- Синхронизация продуктов ---
            var productsFromExcel = new List<Product>();
            var productNamesFromFile = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var row in rowsList1)
            {
                var productName = row.Cell(1).GetString().Trim();
                if (string.IsNullOrWhiteSpace(productName))
                    continue;

                productNamesFromFile.Add(productName);

                var categoryName = row.Cell(6).GetString().Trim();
                if (!categoryDict.TryGetValue(categoryName, out int categoryId))
                {
                    // Категория не найдена, пропускаем продукт или создаём новую категорию
                    continue;
                }

                decimal.TryParse(row.Cell(3).GetString(), out decimal price);
                int.TryParse(row.Cell(5).GetString(), out int stockQuantity);

                var product = new Product
                {
                    ProductName = productName,
                    Description = row.Cell(2).GetString(),
                    Price = price,
                    Currency = row.Cell(4).GetString(),
                    StockQuantity = stockQuantity,
                    CategoryId = categoryId,
                    ImageUrl = row.Cell(7).GetString()
                };

                productsFromExcel.Add(product);
            }

            // Получаем существующие продукты из базы данных
            var existingProducts = context.Products.ToList();

            // Создаём словари для продуктов
            var existingProductNames = new HashSet<string>(existingProducts.Select(p => p.ProductName), StringComparer.OrdinalIgnoreCase);
            var productsFromExcelDict = productsFromExcel.ToDictionary(p => p.ProductName, StringComparer.OrdinalIgnoreCase);

            // Продукты для добавления
            var productsToAdd = productsFromExcel.Where(p => !existingProductNames.Contains(p.ProductName)).ToList();

            // Продукты для удаления
            var productsToDelete = existingProducts.Where(p => !productNamesFromFile.Contains(p.ProductName)).ToList();

            // Добавляем новые продукты
            if (productsToAdd.Any())
            {
                context.Products.AddRange(productsToAdd);
            }

            // Удаляем продукты, которых нет в файле
            if (productsToDelete.Any())
            {
                context.Products.RemoveRange(productsToDelete);
            }

            // Обновляем существующие продукты
            foreach (var existingProduct in existingProducts)
            {
                if (productsFromExcelDict.TryGetValue(existingProduct.ProductName, out var productFromFile))
                {
                    existingProduct.Description = productFromFile.Description;
                    existingProduct.Price = productFromFile.Price;
                    existingProduct.Currency = productFromFile.Currency;
                    existingProduct.StockQuantity = productFromFile.StockQuantity;
                    existingProduct.CategoryId = productFromFile.CategoryId;
                    existingProduct.ImageUrl = productFromFile.ImageUrl;
                }
            }

            // Сохраняем изменения и фиксируем транзакцию
            await context.SaveChangesAsync();
            await transaction.CommitAsync();

            await bot.SendTextMessageAsync(msg.Chat.Id, $"Синхронизация завершена.\n" +
                $"Добавлено категорий: {categoriesToAdd.Count}\n" +
                $"Удалено категорий: {categoriesToDelete.Count}\n" +
                $"Добавлено продуктов: {productsToAdd.Count}\n" +
                $"Удалено продуктов: {productsToDelete.Count}");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            await bot.SendTextMessageAsync(msg.Chat.Id, $"Ошибка при импорте данных: {ex.Message}");
        }
    }
}