using System;
using System.IO;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

public static class MessageHandler
{
    public static async Task SendExcelTemplateAsync(ITelegramBotClient botClient, long chatId)
    {
        // Путь к шаблону Excel-файла
        string templateFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", "Шаблон.xlsx");

        // Текст инструкции
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

        // Отправляем инструкцию
        await botClient.SendTextMessageAsync(chatId, instructionText);

        // Отправляем шаблон файла
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
        var file = await botClient.GetFile(fileId);
        var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Uploads", message.Document.FileName);

        // Создаем директорию Uploads, если она не существует
        Directory.CreateDirectory(Path.GetDirectoryName(filePath));

        // Сохранение файла локально
        using (var saveFileStream = new FileStream(filePath, FileMode.Create))
        {
            await botClient.DownloadFile(file.FilePath, saveFileStream);
        }

        try
        {
            // Импорт данных из Excel-файла
            await DatabaseManager.ImportProductsFromExcelAsync(filePath, botClient, message);

            // Отправляем сообщение об успешном импорте
            await botClient.SendTextMessageAsync(message.Chat.Id, "Данные успешно импортированы.");
        }
        catch (Exception ex)
        {
            // Отправляем сообщение об ошибке
            await botClient.SendTextMessageAsync(message.Chat.Id, $"Произошла ошибка при импорте данных: {ex.Message}");
        }
        finally
        {
            // Удаление файла после обработки
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        }
    }
}
