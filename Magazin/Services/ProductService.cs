using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Magazin.Models;

namespace Magazin.Services
{
    public static class ProductService
    {
        public static async Task SynchronizeProductsAsync(ShopDbContext context, List<Product> productsFromExcel)
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

        // Добавьте другие методы для управления продуктами
    }
}
