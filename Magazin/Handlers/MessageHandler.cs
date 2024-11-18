using Magazin.Services;
using Magazin.Helpers;
using Magazin.Models;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Magazin.Handlers
{
    public class MessageHandler
    {
        // Потокобезопасный словарь для хранения состояния пользователей
        private static readonly ConcurrentDictionary<long, UserState> userStates = new();

        public async Task HandleMessageAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            if (message.Type == MessageType.Text)
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
            else if (message.Type == MessageType.Document)
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

                    var keyboard = KeyboardHelper.GetMainMenuKeyboard();

                    await botClient.SendMessage(
                        chatId: message.Chat.Id,
                        text: "Добро пожаловать! Выберите опцию из меню ниже:",
                        replyMarkup: keyboard,
                        cancellationToken: cancellationToken
                    );

                    // Устанавливаем состояние пользователя в Idle
                    userStates[message.Chat.Id] = UserState.Idle;
                    break;

                // Добавьте обработку других команд

                default:
                    await botClient.SendMessage(
                        chatId: message.Chat.Id,
                        text: "Неизвестная команда.",
                        cancellationToken: cancellationToken
                    );
                    break;
            }
        }

        private async Task HandleTextMessageAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            // Получаем текущее состояние пользователя или устанавливаем по умолчанию Idle
            var state = userStates.GetOrAdd(message.Chat.Id, UserState.Idle);

            if (state == UserState.WaitingForFile)
            {
                if (message.Text.Equals("Отмена", StringComparison.OrdinalIgnoreCase))
                {
                    // Пользователь отменил операцию
                    userStates[message.Chat.Id] = UserState.Idle;

                    var keyboard = KeyboardHelper.GetMainMenuKeyboard();

                    await botClient.SendMessage(
                        chatId: message.Chat.Id,
                        text: "Операция отменена.",
                        replyMarkup: keyboard,
                        cancellationToken: cancellationToken
                    );
                }
                else
                {
                    // Игнорируем другие сообщения
                    await botClient.SendMessage(
                        chatId: message.Chat.Id,
                        text: "Пожалуйста, отправьте файл или нажмите 'Отмена'.",
                        cancellationToken: cancellationToken
                    );
                }
            }
            else
            {
                switch (message.Text)
                {
                    case "🍴 Меню":
                        // Обработка команды "Меню"
                        await botClient.SendMessage(
                            chatId: message.Chat.Id,
                            text: "Здесь будет показано меню продуктов.",
                            cancellationToken: cancellationToken
                        );
                        break;

                    case "📦 Заказы":
                        // Обработка команды "Заказы"
                        await botClient.SendMessage(
                            chatId: message.Chat.Id,
                            text: "Здесь будут отображаться ваши заказы.",
                            cancellationToken: cancellationToken
                        );
                        break;

                    case "🛍 Корзина":
                        // Обработка команды "Корзина"
                        await botClient.SendMessage(
                            chatId: message.Chat.Id,
                            text: "Ваша корзина пуста.",
                            cancellationToken: cancellationToken
                        );
                        break;

                    case "⚙️ Настройки":
                        // Обработка команды "Настройки"
                        await botClient.SendMessage(
                            chatId: message.Chat.Id,
                            text: "Здесь вы можете изменить настройки.",
                            cancellationToken: cancellationToken
                        );
                        break;

                    case "❓ Помощь":
                        // Переводим пользователя в состояние ожидания файла
                        userStates[message.Chat.Id] = UserState.WaitingForFile;

                        var cancelKeyboard = KeyboardHelper.GetCancelKeyboard();

                        await botClient.SendMessage(
                            chatId: message.Chat.Id,
                            text: "Пожалуйста, заполните прилагаемый шаблон Excel-файла и отправьте его мне для импорта данных.\n\n" +
                                  "После заполнения отправьте файл или нажмите 'Отмена' для выхода.",
                            replyMarkup: cancelKeyboard,
                            cancellationToken: cancellationToken
                        );

                        // Отправляем шаблон файла
                        await ExcelService.SendExcelTemplateAsync(botClient, message.Chat.Id);
                        break;

                    // Добавьте обработку других сообщений

                    default:
                        await botClient.SendMessage(
                            chatId: message.Chat.Id,
                            text: "Команда не распознана. Пожалуйста, выберите опцию из меню.",
                            cancellationToken: cancellationToken
                        );
                        break;
                }
            }
        }

        private async Task HandleDocumentAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
        {
            // Получаем текущее состояние пользователя или устанавливаем по умолчанию Idle
            var state = userStates.GetOrAdd(message.Chat.Id, UserState.Idle);

            if (state == UserState.WaitingForFile)
            {
                try
                {
                    // Обрабатываем полученный файл
                    await ExcelService.HandleReceivedDocumentAsync(botClient, message);

                    // Устанавливаем состояние пользователя в Idle
                    userStates[message.Chat.Id] = UserState.Idle;

                    var keyboard = KeyboardHelper.GetMainMenuKeyboard();

                    await botClient.SendMessage(
                        chatId: message.Chat.Id,
                        text: "Данные успешно импортированы.",
                        replyMarkup: keyboard,
                        cancellationToken: cancellationToken
                    );
                }
                catch (Exception ex)
                {
                    await botClient.SendMessage(
                        chatId: message.Chat.Id,
                        text: $"Произошла ошибка при импорте данных: {ex.Message}",
                        cancellationToken: cancellationToken
                    );
                }
            }
            else
            {
                await botClient.SendMessage(
                    chatId: message.Chat.Id,
                    text: "Я не ожидаю от вас файл. Пожалуйста, нажмите кнопку '❓ Помощь', чтобы получить инструкцию.",
                    cancellationToken: cancellationToken
                );
            }
        }
    }
}
