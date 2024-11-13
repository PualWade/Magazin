using Magazin.Services;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Magazin.Services;

namespace Magazin.Handlers
{
    public class MessageHandler
    {
        // Потокобезопасный словарь для хранения состояния пользователей
        private static readonly ConcurrentDictionary<long, bool> userWaitingForFile = new();

        public async Task HandleMessageAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            if (message.Text != null)
            {
                if (message.Text.StartsWith('/'))
                {
                    await HandleCommandAsync(botClient, message, cancellationToken);
                }
                else
                {
                    await HandleTextMessageAsync(botClient, message, cancellationToken);
                }
            }
            else if (message.Document != null)
            {
                await HandleDocumentAsync(botClient, message, cancellationToken);
            }
            else
            {
                Console.WriteLine($"Получено сообщение типа {message.Type}");
            }
        }

        private async Task HandleCommandAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            var command = message.Text.Split(' ')[0].ToLower();

            switch (command)
            {
                case "/start":
                    await UserService.AddUserAsync(message.From);
                    await botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text: "Добро пожаловать! Используйте меню для навигации.",
                        replyMarkup: new ReplyKeyboardRemove(),
                        cancellationToken: cancellationToken
                    );
                    break;
                // Добавьте обработку других команд
                default:
                    await botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text: "Неизвестная команда.",
                        cancellationToken: cancellationToken
                    );
                    break;
            }
        }

        private async Task HandleTextMessageAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            switch (message.Text)
            {
                case "❓ Помощь":
                    await ExcelService.SendExcelTemplateAsync(botClient, message.Chat.Id);
                    userWaitingForFile[message.Chat.Id] = true;
                    break;
                // Добавьте обработку других сообщений
                default:
                    await botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text: "Команда не распознана.",
                        parseMode: ParseMode.Html,
                        cancellationToken: cancellationToken
                    );
                    break;
            }
        }

        private async Task HandleDocumentAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            if (userWaitingForFile.TryGetValue(message.Chat.Id, out bool isWaiting) && isWaiting)
            {
                await ExcelService.HandleReceivedDocumentAsync(botClient, message);
                userWaitingForFile[message.Chat.Id] = false;
            }
            else
            {
                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: "Я не ожидаю от вас файл. Пожалуйста, нажмите кнопку '❓ Помощь', чтобы получить инструкцию.",
                    cancellationToken: cancellationToken
                );
            }
        }
    }
}
