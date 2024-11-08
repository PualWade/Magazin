using Telegram.Bot.Types;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;


public static class DatabaseManager
{
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
}
