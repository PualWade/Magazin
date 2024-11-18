using System;
using System.IO;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;


namespace Magazin.Services
{
    public static class ExcelService
    {
        public static async Task SendExcelTemplateAsync(ITelegramBotClient botClient, long chatId)
        {
            string templateFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", "Шаблон.xlsx");

            if (System.IO.File.Exists(templateFilePath))
            {
                using var fileStream = new FileStream(templateFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                await botClient.SendDocument(
                    chatId: chatId,
                    document: new InputFileStream(fileStream, "Шаблон.xlsx"),
                    caption: "Шаблон для импорта продуктов и категорий"
                );
            }
            else
            {
                await botClient.SendMessage(chatId, "Шаблон не найден.");
            }
        }

        public static async Task HandleReceivedDocumentAsync(ITelegramBotClient botClient, Message message)
        {
            var fileId = message.Document.FileId;
            var file = await botClient.GetFile(fileId);
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Uploads", message.Document.FileName);

            Directory.CreateDirectory(Path.GetDirectoryName(filePath));

            using (var saveFileStream = new FileStream(filePath, FileMode.Create))
            {
                await botClient.DownloadFile(file.FilePath, saveFileStream);
            }

            try
            {
                // Импорт данных из Excel-файла
                await DatabaseService.ImportProductsFromExcelAsync(filePath, botClient, message);

                // Отправка сообщения об успешном импорте перенесена в MessageHandler
            }
            finally
            {
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }
        }
    }
}
