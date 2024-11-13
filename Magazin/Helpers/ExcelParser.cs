using System;
using System.Collections.Generic;
using ClosedXML.Excel;
using Magazin.Models;

namespace Magazin.Helpers
{
    public static class ExcelParser
    {
        public static List<Category> GetCategoriesFromSheet(IXLWorksheet worksheet)
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

        public static List<Product> GetProductsFromSheet(IXLWorksheet worksheet, Dictionary<string, int> categoryDict)
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
    }
}
