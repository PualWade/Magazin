using System.Threading.Tasks;
using Magazin.Models;

namespace Magazin.Services
{
    public static class UserService
    {
        public static async Task AddUserAsync(Telegram.Bot.Types.User user)
        {
            using var context = new ShopDbContext();

            var userRole = context.Roles.FirstOrDefault(r => r.RoleName == "Пользователь");

            if (!context.Users.Any(u => u.UserId == user.Id))
            {
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
                await context.SaveChangesAsync();
            }
        }

        // Добавьте другие методы для управления пользователями
    }
}
