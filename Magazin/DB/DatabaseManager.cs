using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using ClosedXML.Excel;

public static class DatabaseManager
{
    // Добавляет пользователя в БД
    public static void AddUser(Telegram.Bot.Types.User user)
    {
        using var context = new ShopDbContext();

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
                RegistrationDate = DateTime.UtcNow
            });
            context.SaveChanges();
        }
    }

    // Заполняет роли пользователей
    public static void AddRoles()
    {
        using var context = new ShopDbContext();

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

    // Импортирует данные из Excel-файла
    public static async Task ImportProductsFromExcelAsync(string filePath, ITelegramBotClient botClient, Message msg)
    {
        using var workbook = new XLWorkbook(filePath);
        using var context = new ShopDbContext();
        using var transaction = await context.Database.BeginTransactionAsync();

        try
        {
            // Чтение данных из Excel
            var productSheet = workbook.Worksheet("Продукты");
            var categorySheet = workbook.Worksheet("Категории");

            // Синхронизация категорий
            var categoriesFromExcel = GetCategoriesFromSheet(categorySheet);
            await SynchronizeCategoriesAsync(context, categoriesFromExcel);

            // Синхронизация продуктов
            var categoriesDict = context.Categories.ToDictionary(c => c.CategoryName, c => c.CategoryId, StringComparer.OrdinalIgnoreCase);
            var productsFromExcel = GetProductsFromSheet(productSheet, categoriesDict);
            await SynchronizeProductsAsync(context, productsFromExcel);

            // Фиксируем транзакцию
            await transaction.CommitAsync();

            await botClient.SendTextMessageAsync(msg.Chat.Id, "Данные успешно синхронизированы.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            await botClient.SendTextMessageAsync(msg.Chat.Id, $"Ошибка при импорте данных: {ex.Message}");
            Console.WriteLine(ex.Message.ToString());
        }
    }

    private static List<Category> GetCategoriesFromSheet(IXLWorksheet worksheet)
    {
        var categories = new List<Category>();
        var categoryNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var rows = worksheet.RowsUsed().Skip(1); // Пропускаем заголовок

        foreach (var row in rows)
        {
            var categoryName = row.Cell(1).GetString().Trim();

            if (!string.IsNullOrWhiteSpace(categoryName) && categoryNames.Add(categoryName))
            {
                categories.Add(new Category
                {
                    CategoryName = categoryName,
                    Description = row.Cell(2).GetString().Trim()
                });
            }
        }

        return categories;
    }

    private static async Task SynchronizeCategoriesAsync(ShopDbContext context, List<Category> categoriesFromExcel)
    {
        var existingCategories = context.Categories.ToList();

        var categoryNamesFromFile = new HashSet<string>(categoriesFromExcel.Select(c => c.CategoryName), StringComparer.OrdinalIgnoreCase);
        var existingCategoryNames = new HashSet<string>(existingCategories.Select(c => c.CategoryName), StringComparer.OrdinalIgnoreCase);

        var categoriesToAdd = categoriesFromExcel.Where(c => !existingCategoryNames.Contains(c.CategoryName)).ToList();
        var categoriesToDelete = existingCategories.Where(c => !categoryNamesFromFile.Contains(c.CategoryName)).ToList();

        if (categoriesToAdd.Any())
        {
            context.Categories.AddRange(categoriesToAdd);
        }

        if (categoriesToDelete.Any())
        {
            context.Categories.RemoveRange(categoriesToDelete);
        }

        foreach (var category in existingCategories)
        {
            var categoryFromFile = categoriesFromExcel.FirstOrDefault(c => c.CategoryName.Equals(category.CategoryName, StringComparison.OrdinalIgnoreCase));
            if (categoryFromFile != null)
            {
                category.Description = categoryFromFile.Description;
            }
        }

        await context.SaveChangesAsync();
    }

    private static List<Product> GetProductsFromSheet(IXLWorksheet worksheet, Dictionary<string, int> categoryDict)
    {
        var products = new List<Product>();
        var productNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var rows = worksheet.RowsUsed().Skip(1); // Пропускаем заголовок

        foreach (var row in rows)
        {
            var productName = row.Cell(1).GetString().Trim();
            if (string.IsNullOrWhiteSpace(productName))
                continue;

            if (productNames.Add(productName))
            {
                var categoryName = row.Cell(6).GetString().Trim();
                if (!categoryDict.TryGetValue(categoryName, out int categoryId))
                {
                    continue; // Категория не найдена
                }

                decimal.TryParse(row.Cell(3).GetString(), out decimal price);
                int.TryParse(row.Cell(5).GetString(), out int stockQuantity);

                products.Add(new Product
                {
                    ProductName = productName,
                    Description = row.Cell(2).GetString().Trim(),
                    Price = price,
                    Currency = row.Cell(4).GetString().Trim(),
                    StockQuantity = stockQuantity,
                    CategoryId = categoryId,
                    ImageUrl = row.Cell(7).GetString().Trim()
                });
            }
        }

        return products;
    }

    private static async Task SynchronizeProductsAsync(ShopDbContext context, List<Product> productsFromExcel)
    {
        var existingProducts = context.Products.ToList();

        var productNamesFromFile = new HashSet<string>(productsFromExcel.Select(p => p.ProductName), StringComparer.OrdinalIgnoreCase);
        var existingProductNames = new HashSet<string>(existingProducts.Select(p => p.ProductName), StringComparer.OrdinalIgnoreCase);

        var productsToAdd = productsFromExcel.Where(p => !existingProductNames.Contains(p.ProductName)).ToList();
        var productsToDelete = existingProducts.Where(p => !productNamesFromFile.Contains(p.ProductName)).ToList();

        if (productsToAdd.Any())
        {
            context.Products.AddRange(productsToAdd);
        }

        if (productsToDelete.Any())
        {
            context.Products.RemoveRange(productsToDelete);
        }

        foreach (var existingProduct in existingProducts)
        {
            var productFromFile = productsFromExcel.FirstOrDefault(p => p.ProductName.Equals(existingProduct.ProductName, StringComparison.OrdinalIgnoreCase));
            if (productFromFile != null)
            {
                existingProduct.Description = productFromFile.Description;
                existingProduct.Price = productFromFile.Price;
                existingProduct.Currency = productFromFile.Currency;
                existingProduct.StockQuantity = productFromFile.StockQuantity;
                existingProduct.CategoryId = productFromFile.CategoryId;
                existingProduct.ImageUrl = productFromFile.ImageUrl;
            }
        }

        await context.SaveChangesAsync();
    }
}
