using Magazin.Services;
using System;
using System.IO;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Magazin.Services;

namespace Magazin.Services
{
    public static class ExcelService
    {
        public static async Task SendExcelTemplateAsync(ITelegramBotClient botClient, long chatId)
        {
            string templateFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", "Шаблон.xlsx");

            string instructionText = "Пожалуйста, заполните прилагаемый шаблон Excel-файла и отправьте его мне для импорта данных.\n\n" +
                                     "Инструкция по заполнению:\n" +
                                     "1. Лист 'Продукты':\n" +
                                     "   - Столбец A: Название продукта\n" +
                                     "   - Столбец B: Описание\n" +
                                     "   - Столбец C: Цена\n" +
                                     "   - Столбец D: Валюта\n" +
                                     "   - Столбец E: Количество на складе\n" +
                                     "   - Столбец F: Название категории\n" +
                                     "   - Столбец G: URL изображения\n" +
                                     "2. Лист 'Категории':\n" +
                                     "   - Столбец A: Название категории\n" +
                                     "   - Столбец B: Описание категории\n\n" +
                                     "После заполнения отправьте файл в ответ на это сообщение.";

            await botClient.SendTextMessageAsync(chatId, instructionText);

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
                await botClient.SendTextMessageAsync(chatId, "Шаблон не найден.");
            }
        }

        public static async Task HandleReceivedDocumentAsync(ITelegramBotClient botClient, Message message)
        {
            var fileId = message.Document.FileId;
            var file = await botClient.GetFileAsync(fileId);
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Uploads", message.Document.FileName);

            Directory.CreateDirectory(Path.GetDirectoryName(filePath));

            using (var saveFileStream = new FileStream(filePath, FileMode.Create))
            {
                await botClient.DownloadFile(file.FilePath, saveFileStream);
            }

            try
            {
                await DatabaseService.ImportProductsFromExcelAsync(filePath, botClient, message);

                await botClient.SendTextMessageAsync(message.Chat.Id, "Данные успешно импортированы.");
            }
            catch (Exception ex)
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, $"Произошла ошибка при импорте данных: {ex.Message}");
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
