using System;
using System.IO;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Magazin.Services
{
    public static class DocumentService
    {
        public static async Task SendTemplateFilesAsync(ITelegramBotClient botClient, long chatId)
        {
            // Отправка шаблона Excel
            string excelTemplatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", "Шаблон.xlsx");
            if (System.IO.File.Exists(excelTemplatePath))
            {
                using var excelStream = new FileStream(excelTemplatePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                await botClient.SendDocument(
                    chatId: chatId,
                    document: new InputFileStream(excelStream, "Шаблон.xlsx"),
                    caption: "Шаблон для импорта продуктов и категорий (Excel)"
                );
            }

            // Отправка шаблона CSV
            string csvTemplatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", "Шаблон.csv");
            if (System.IO.File.Exists(csvTemplatePath))
            {
                using var csvStream = new FileStream(csvTemplatePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                await botClient.SendDocument(
                    chatId: chatId,
                    document: new InputFileStream(csvStream, "Шаблон.csv"),
                    caption: "Шаблон для импорта продуктов и категорий (CSV)"
                );
            }
        }

        public static async Task HandleReceivedExcelAsync(ITelegramBotClient botClient, Message message)
        {
            var filePath = await DownloadFileAsync(botClient, message);

            try
            {
                // Обработка Excel-файла
                await DatabaseService.ImportProductsFromExcelAsync(filePath,botClient,message);
            }
            finally
            {
                DeleteFile(filePath);
            }
        }

        public static async Task HandleReceivedCsvAsync(ITelegramBotClient botClient, Message message)
        {
            var filePath = await DownloadFileAsync(botClient, message);

            try
            {
                // Обработка CSV-файла
                await DatabaseService.ImportProductsFromCsvAsync(filePath, botClient, message);
            }
            finally
            {
                DeleteFile(filePath);
            }
        }

        private static async Task<string> DownloadFileAsync(ITelegramBotClient botClient, Message message)
        {
            var fileId = message.Document.FileId;
            var fileInfo = await botClient.GetFile(fileId);
            var fileExtension = Path.GetExtension(message.Document.FileName);
            var fileName = $"upload_{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Uploads", fileName);

            Directory.CreateDirectory(Path.GetDirectoryName(filePath));

            using (var saveFileStream = new FileStream(filePath, FileMode.Create))
            {
                await botClient.DownloadFile(fileInfo.FilePath, saveFileStream);
            }

            return filePath;
        }

        private static void DeleteFile(string filePath)
        {
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        }
    }
}



