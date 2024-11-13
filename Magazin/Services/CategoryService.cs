using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Magazin.Models;

namespace Magazin.Services
{
    public static class CategoryService
    {
        public static async Task SynchronizeCategoriesAsync(ShopDbContext context, List<Category> categoriesFromExcel)
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

        // Добавьте другие методы для управления категориями
    }
}
