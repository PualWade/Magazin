
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ClosedXML.Excel;
using CsvHelper;
using CsvHelper.Configuration;
using Magazin.Helpers;
using Magazin.Models;
using Telegram.Bot.Types;
using Telegram.Bot;

namespace Magazin.Services
{
    public static class DatabaseService
    {
        public static async Task ImportProductsFromExcelAsync(string filePath, ITelegramBotClient botClient, Message msg)
        {
            using var workbook = new XLWorkbook(filePath);
            using var context = new ShopDbContext();
            using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                var productSheet = workbook.Worksheet("Продукты");
                var categorySheet = workbook.Worksheet("Категории");

                var categoriesFromExcel = DocumentParser.GetCategoriesFromExcelSheet(categorySheet);
                await CategoryService.SynchronizeCategoriesAsync(context, categoriesFromExcel);

                var categoriesDict = context.Categories.ToDictionary(c => c.CategoryName, c => c.CategoryId, StringComparer.OrdinalIgnoreCase);
                var productsFromExcel = DocumentParser.GetProductsFromExcelSheet(productSheet, categoriesDict);
                await ProductService.SynchronizeProductsAsync(context, productsFromExcel);

                await transaction.CommitAsync();

                await botClient.SendMessage(msg.Chat.Id, "Данные успешно синхронизированы.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await botClient.SendMessage(msg.Chat.Id, $"Ошибка при импорте данных: {ex.Message}");
            }
        }

        public static async Task ImportProductsFromCsvAsync(string filePath, ITelegramBotClient botClient, Message msg)
        {
            using var reader = new StreamReader(filePath);
            var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ",",
                HasHeaderRecord = true,
                MissingFieldFound = null,
                BadDataFound = null,
                HeaderValidated = null // Отключаем проверку заголовков, если нужно
            };
            using var csv = new CsvReader(reader, csvConfig);

            var records = csv.GetRecords<CsvProductModel>().ToList();

            using var context = new ShopDbContext();
            using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                // Получаем список категорий из CSV-файла
                var categoriesFromCsv = records.Select(r => new Category
                {
                    CategoryName = r.CategoryName.Trim(),
                    Description = r.CategoryDescription?.Trim()
                }).Distinct(new CategoryComparer()).ToList();

                // Получаем список категорий из базы данных
                var existingCategories = context.Categories.ToList();

                // Определяем категории для добавления
                var categoriesToAdd = categoriesFromCsv
                    .Where(c => !existingCategories.Any(ec => ec.CategoryName.Equals(c.CategoryName, StringComparison.OrdinalIgnoreCase)))
                    .ToList();

                // Определяем категории для удаления
                var categoriesToDelete = existingCategories
                    .Where(ec => !categoriesFromCsv.Any(c => c.CategoryName.Equals(ec.CategoryName, StringComparison.OrdinalIgnoreCase)))
                    .ToList();

                // Определяем категории для обновления
                var categoriesToUpdate = existingCategories
                    .Where(ec => categoriesFromCsv.Any(c => c.CategoryName.Equals(ec.CategoryName, StringComparison.OrdinalIgnoreCase)))
                    .ToList();

                // Добавляем новые категории
                if (categoriesToAdd.Any())
                {
                    context.Categories.AddRange(categoriesToAdd);
                }

                // Удаляем категории, которых нет в файле
                if (categoriesToDelete.Any())
                {
                    context.Categories.RemoveRange(categoriesToDelete);
                }

                // Обновляем существующие категории (если нужно обновлять описание)
                foreach (var existingCategory in categoriesToUpdate)
                {
                    var categoryFromFile = categoriesFromCsv.First(c => c.CategoryName.Equals(existingCategory.CategoryName, StringComparison.OrdinalIgnoreCase));
                    existingCategory.Description = categoryFromFile.Description;
                }

                // Сохраняем изменения в категориях
                await context.SaveChangesAsync();

                // Обновляем словарь категорий после изменений
                var categoriesDict = context.Categories.ToDictionary(c => c.CategoryName, c => c.CategoryId, StringComparer.OrdinalIgnoreCase);

                // Получаем список продуктов из CSV-файла
                var productsFromCsv = records.Select(r => new Product
                {
                    ProductName = r.ProductName.Trim(),
                    Description = r.Description?.Trim(),
                    Price = r.Price,
                    Currency = r.Currency?.Trim(),
                    StockQuantity = r.StockQuantity,
                    CategoryId = categoriesDict[r.CategoryName.Trim()],
                    ImageUrl = r.ImageUrl?.Trim(),
                    IsActive = r.IsActive
                }).ToList();

                // Получаем список продуктов из базы данных
                var existingProducts = context.Products.ToList();

                // Определяем продукты для добавления
                var productsToAdd = productsFromCsv
                    .Where(p => !existingProducts.Any(ep => ep.ProductName.Equals(p.ProductName, StringComparison.OrdinalIgnoreCase)))
                    .ToList();

                // Определяем продукты для удаления
                var productsToDelete = existingProducts
                    .Where(ep => !productsFromCsv.Any(p => p.ProductName.Equals(ep.ProductName, StringComparison.OrdinalIgnoreCase)))
                    .ToList();

                // Определяем продукты для обновления
                var productsToUpdate = existingProducts
                    .Where(ep => productsFromCsv.Any(p => p.ProductName.Equals(ep.ProductName, StringComparison.OrdinalIgnoreCase)))
                    .ToList();

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
                foreach (var existingProduct in productsToUpdate)
                {
                    var productFromFile = productsFromCsv.First(p => p.ProductName.Equals(existingProduct.ProductName, StringComparison.OrdinalIgnoreCase));
                    existingProduct.Description = productFromFile.Description;
                    existingProduct.Price = productFromFile.Price;
                    existingProduct.Currency = productFromFile.Currency;
                    existingProduct.StockQuantity = productFromFile.StockQuantity;
                    existingProduct.CategoryId = productFromFile.CategoryId;
                    existingProduct.ImageUrl = productFromFile.ImageUrl;
                    existingProduct.IsActive = productFromFile.IsActive;
                }

                // Сохраняем изменения в продуктах
                await context.SaveChangesAsync();

                // Фиксируем транзакцию
                await transaction.CommitAsync();

                await botClient.SendMessage(msg.Chat.Id, "Данные успешно синхронизированы.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await botClient.SendMessage(msg.Chat.Id, $"Ошибка при импорте данных: {ex.Message}");
            }
        }


        // Добавьте другие методы для работы с базой данных
    }
}




