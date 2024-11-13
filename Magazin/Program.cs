using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Polling;

class Program
{
    // Потокобезопасный словарь для хранения состояния пользователей
    private static readonly ConcurrentDictionary<long, bool> userWaitingForFile = new();

    // Объявляем botClient как статическое поле
    private static ITelegramBotClient botClient;

    static async Task Main(string[] args)
    {
        await RunBotAsync();
    }

    static async Task RunBotAsync()
    {
        var token = Environment.GetEnvironmentVariable("TOKEN") ?? "YOUR_BOT_TOKEN";

        botClient = new TelegramBotClient(token);

        using var cts = new CancellationTokenSource();

        // Создаём экземпляр UpdateHandler
        var updateHandler = new UpdateHandler();

        // Запускаем получение обновлений
        botClient.StartReceiving(
            updateHandler,
            receiverOptions: new ReceiverOptions(),
            cancellationToken: cts.Token
        );

        var me = await botClient.GetMeAsync();
        Console.WriteLine($"Бот @{me.Username} запущен. Нажмите любую клавишу для выхода.");
        Console.ReadKey();

        // Останавливаем бота
        cts.Cancel();
    }

    public static async Task ProcessMessageAsync(ITelegramBotClient botClient, Message msg, CancellationToken cancellationToken)
    {
        if (msg.Text != null)
        {
            if (msg.Text.StartsWith('/'))
            {
                await OnCommandAsync(botClient, msg, cancellationToken);
            }
            else
            {
                await OnTextMessageAsync(botClient, msg, cancellationToken);
            }
        }
        else if (msg.Document != null)
        {
            await OnDocumentReceivedAsync(botClient, msg, cancellationToken);
        }
        else
        {
            Console.WriteLine($"Получено сообщение типа {msg.Type}");
        }
    }


    private static async Task OnTextMessageAsync(ITelegramBotClient botClient, Message msg, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Получен текст '{msg.Text}' от {msg.Chat.Id}");
        switch (msg.Text)
        {
            case "❓ Помощь":
                await MessageHandler.SendExcelTemplateAsync(botClient, msg.Chat.Id);
                userWaitingForFile[msg.Chat.Id] = true;
                break;
            // Добавьте обработку других сообщений
            default:
                await botClient.SendTextMessageAsync(msg.Chat.Id, "Команда не распознана.", parseMode: ParseMode.Html, cancellationToken: cancellationToken);
                break;
        }
    }

    private static async Task OnCommandAsync(ITelegramBotClient botClient, Message msg, CancellationToken cancellationToken)
    {
        var text = msg.Text;
        var space = text.IndexOf(' ');
        if (space < 0) space = text.Length;
        var command = text[..space].ToLower();

        Console.WriteLine($"Получена команда: {command}");

        switch (command)
        {
            case "/start":
                DatabaseManager.AddRoles();
                DatabaseManager.AddUser(msg.From);
                await botClient.SendTextMessageAsync(msg.Chat.Id, "Добро пожаловать! Используйте меню для навигации.", replyMarkup: new ReplyKeyboardRemove(), cancellationToken: cancellationToken);
                break;
            // Добавьте обработку других команд
            default:
                await botClient.SendTextMessageAsync(msg.Chat.Id, "Неизвестная команда.", cancellationToken: cancellationToken);
                break;
        }
    }

    private static async Task OnDocumentReceivedAsync(ITelegramBotClient botClient, Message msg, CancellationToken cancellationToken)
    {
        if (userWaitingForFile.TryGetValue(msg.Chat.Id, out bool isWaiting) && isWaiting)
        {
            await MessageHandler.HandleReceivedDocumentAsync(botClient, msg);
            userWaitingForFile[msg.Chat.Id] = false;
        }
        else
        {
            await botClient.SendTextMessageAsync(msg.Chat.Id, "Я не ожидаю от вас файл. Пожалуйста, нажмите кнопку '❓ Помощь', чтобы получить инструкцию.", cancellationToken: cancellationToken);
        }
    }

    public static async Task HandleCallbackQueryAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
    {
        await botClient.AnswerCallbackQueryAsync(callbackQuery.Id, $"Вы выбрали {callbackQuery.Data}", cancellationToken: cancellationToken);
        await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, $"Обработан callback: {callbackQuery.Data}", cancellationToken: cancellationToken);
    }
}
