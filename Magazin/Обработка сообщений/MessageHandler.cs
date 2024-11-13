using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot;
using System.IO;

public static class MessageHandler
{
    public static async Task SendExcelTemplateAsync(TelegramBotClient bot, ChatId chatId)
    {
        // Путь к шаблону Excel-файла
        string templateFilePath = Path.Combine("Templates", "Шаблон.xlsx");

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
        await bot.SendTextMessageAsync(chatId, instructionText);

        // Отправляем шаблон файла
        using (var fileStream = new FileStream(templateFilePath, FileMode.Open, FileAccess.Read))
        {
            await bot.SendDocument(
                chatId: chatId,
                document: new InputFileStream(fileStream, "Шаблон.xlsx"),
                caption: "Шаблон для импорта продуктов и категорий"
            );
        }
    }


    public static async Task HandleReceivedDocumentAsync(TelegramBotClient bot, Message message)
    {
        var fileId = message.Document.FileId;
        var file = await bot.GetFile(fileId);
        var filePath = Path.Combine("Uploads", message.Document.FileName);

        // Создаем директорию Uploads, если она не существует
        Directory.CreateDirectory("Uploads");

        // Сохранение файла локально
        using (var saveFileStream = new FileStream(filePath, FileMode.Create))
        {
            await bot.DownloadFile(file.FilePath, saveFileStream);
        }

        try
        {
            // Импорт данных из Excel-файла
            await DatabaseManager.ImportProductsFromExcel(filePath, bot, message);

            // Отправляем сообщение об успешном импорте
            await bot.SendTextMessageAsync(message.Chat.Id, "Данные успешно импортированы.");
        }
        catch (Exception ex)
        {
            // Отправляем сообщение об ошибке
            await bot.SendTextMessageAsync(message.Chat.Id, $"Произошла ошибка при импорте данных: {ex.Message}");
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
