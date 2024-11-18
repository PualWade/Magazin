using System;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using ClosedXML.Excel;
using Magazin.Models;
using Magazin.Services;
using Magazin.Helpers;

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

                var categoriesFromExcel = ExcelParser.GetCategoriesFromSheet(categorySheet);
                await CategoryService.SynchronizeCategoriesAsync(context, categoriesFromExcel);

                var categoriesDict = context.Categories.ToDictionary(c => c.CategoryName, c => c.CategoryId, StringComparer.OrdinalIgnoreCase);
                var productsFromExcel = ExcelParser.GetProductsFromSheet(productSheet, categoriesDict);
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

        // Добавьте другие методы для работы с базой данных
    }
}
