using System;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using System.Collections.Generic;

class Program
{
    // Словарь для хранения состояния пользователей
    private static Dictionary<long, bool> userWaitingForFile = new Dictionary<long, bool>();

    static async Task Main(string[] args)
    {
        await RunBotAsync();
    }
    static async Task RunBotAsync()
    {
        // замените YOUR_BOT_TOKEN ниже или установите свой ТОКЕН в свойствах проекта > Отладка > Пользовательский интерфейс профилей запуска > Переменные среды
        var token = Environment.GetEnvironmentVariable("TOKEN") ?? "YOUR_BOT_TOKEN";

        using var cts = new CancellationTokenSource();
        var bot = new TelegramBotClient(token, cancellationToken: cts.Token);
        var me = await bot.GetMe();
        //await bot.DeleteWebhook();          // Удаляет вебхук (если он был установлен) и переводит бота в режим Long Polling для получения обновлений. Это полезно, если бот должен работать без вебхука и получать сообщения напрямую от сервера Telegram. Если ваш бот изначально не использует вебхук, эту строку можно закомментировать.
        //await bot.DropPendingUpdates();     //  Удаляет все ожидающие обновления. Это может быть полезно, если вы хотите начать с чистого списка сообщений и не обрабатывать накопленные, особенно при первом запуске бота или после длительного простоя. Закомментировать эту строку можно, если вы хотите, чтобы бот обрабатывал все старые сообщения и обновления.
        bot.OnError += OnError;
        bot.OnMessage += OnMessage;
        bot.OnUpdate += OnUpdate;

        Console.WriteLine($"@{me.Username} выполняется... Нажмите клавишу Escape для завершения");
        while (Console.ReadKey(true).Key != ConsoleKey.Escape) ;
        cts.Cancel(); // stop the bot

        // Этот метод OnError обрабатывает ошибки, возникающие при работе бота.
        async Task OnError(Exception exception, HandleErrorSource source)
        {
            Console.WriteLine(exception);
            await Task.Delay(2000, cts.Token);
        }

        async Task OnMessage(Message msg, UpdateType type)
        {
            if (msg.Text is not { } text)
                if (msg.Document != null)
                {
                    await OnDocumentReceived(msg);
                }
                else
                {
                    Console.WriteLine($"Получено сообщение типа {msg.Type}");

                }
            else if (text.StartsWith('/'))
            {
                var space = text.IndexOf(' ');
                if (space < 0) space = text.Length;
                var command = text[..space].ToLower();
                if (command.LastIndexOf('@') is > 0 and int at) // это целенаправленная команда
                    if (command[(at + 1)..].Equals(me.Username, StringComparison.OrdinalIgnoreCase))
                        command = command[..at];
                    else
                        return; // команда для другого бота
                await OnCommand(command, text[space..].TrimStart(), msg);
            }
            else
                await OnTextMessage(msg);
        }

        async Task OnTextMessage(Message msg) // получено текстовое сообщение, которое не является командой
        {
            Console.WriteLine($"Полученный текст '{msg.Text}' в {msg.Chat}");
            switch (msg.Text)
            {
                case "🍴 Меню":
                    
                    break;
                case "📦 Заказы":

                    break;
                case "🛍 Корзина":

                    break;
                case "⚙️ Настройки":

                    break;
                case "❓ Помощь":
                    await MessageHandler.SendExcelTemplateAsync(bot,msg.Chat.Id);
                    //DatabaseManager.ImportProductsFromExcel(@"C:\Users\User\Downloads\Списокк товаров на двух листах.xlsx", bot, msg);
                    userWaitingForFile[msg.Chat.Id] = true;
                    break;
                case "👸 Для Лизочки":
                    if (7243188298 == msg.Chat.Id)
                    {
                        await bot.SetMessageReaction(msg.Chat, msg.Id, ["❤"], false);
                        await bot.SendMessage(msg.Chat, "Доброй ночи красотка 👸");
                    }
                    else {
                        await bot.SetMessageReaction(msg.Chat, msg.Id, ["💩"], false);
                        await bot.SendMessage(msg.Chat, "Ухади, я жду Лизочку ❤️");
                    }
                    break;
                case "👸 Для Василиски":
                    if (7202475803 == msg.Chat.Id)
                    {
                        await bot.SetMessageReaction(msg.Chat, msg.Id, ["❤"], false);
                        await bot.SendMessage(msg.Chat, "Доброй ночи красотка 👸");
                    }
                    else
                    {
                        await bot.SetMessageReaction(msg.Chat, msg.Id, ["💩"], false);
                        await bot.SendMessage(msg.Chat, "Ухади, я жду Василиску ❤️");
                    }
                    break;
                default:
                    await bot.SendMessage(msg.Chat, "Кнопки клавиатури изменени", parseMode: ParseMode.Html);
                    break;
            }
            //await OnCommand("/start", "", msg); // на данный момент мы перенаправляемся на команду /start
        }

        async Task OnCommand(string command, string args, Message msg)
        {
            Console.WriteLine($"Получена команда: {command} {args}");
            switch (command)
            {
                case "/start":
                    DatabaseManager.AddRoles();
                    DatabaseManager.AddUser(msg.From);
                    await bot.SendMessage(msg.Chat, """
                <b><u>Bot menu</u></b>:
                /photo [url]    - send a photo <i>(optionally from an <a href="https://picsum.photos/310/200.jpg">url</a>)</i>
                /inline_buttons - send inline buttons
                /keyboard       - send keyboard buttons
                /remove         - remove keyboard buttons
                /poll           - send a poll
                /reaction       - send a reaction
                """, parseMode: ParseMode.Html, linkPreviewOptions: true,
                        replyMarkup: new ReplyKeyboardRemove()); // также удалите клавиатуру, чтобы навести порядок
                    break;
                case "/photo":
                    if (args.StartsWith("http"))
                        await bot.SendPhoto(msg.Chat, args, caption: "Source: " + args);
                    else
                    {
                        await bot.SendChatAction(msg.Chat, ChatAction.UploadPhoto);
                        await Task.Delay(2000); // simulate a long task
                        await using var fileStream = new FileStream("bot.gif", FileMode.Open, FileAccess.Read);
                        await bot.SendPhoto(msg.Chat, fileStream, caption: "Прочитай https://avatars.mds.yandex.net/i?id=021ad6b68f75d3545a93af0aee2ca315537dea5e-12494025-images-thumbs&n=13");
                    }
                    break;
                case "/inline_buttons":
                    var inlineMarkup = new InlineKeyboardMarkup()
                        .AddNewRow("1.1", "1.2", "1.3")
                        .AddNewRow()
                            .AddButton("WithCallbackData", "CallbackData")
                            .AddButton(InlineKeyboardButton.WithUrl("WithUrl", "https://avatars.mds.yandex.net/i?id=021ad6b68f75d3545a93af0aee2ca315537dea5e-12494025-images-thumbs&n=13"));
                    await bot.SendMessage(msg.Chat, "Встроенные кнопки:", replyMarkup: inlineMarkup);
                    break;
                case "/keyboard":
                    var replyMarkup = new ReplyKeyboardMarkup()
                        .AddNewRow("🍴 Меню", "📦 Заказы", "🛍 Корзина")
                        .AddNewRow().AddButton("⚙️ Настройки").AddButton("❓ Помощь")
                        .AddNewRow("👸 Для Лизочки","👸 Для Василиски");
                    await bot.SendMessage(msg.Chat, "Кнопки клавиатуры:", replyMarkup: replyMarkup);
                    break;
                case "/remove":
                    await bot.SendMessage(msg.Chat, "Извлечение клавиатуры", replyMarkup: new ReplyKeyboardRemove());
                    break;
                case "/poll":
                    await bot.SendPoll(msg.Chat, "Вопрос", ["Вариант 0", "Вариант 1", "Вариант 2"], isAnonymous: false, allowsMultipleAnswers: true);
                    break;
                case "/reaction":
                    await bot.SetMessageReaction(msg.Chat, msg.Id, ["❤"], false);
                    break;
            }
        }

        async Task OnUpdate(Update update)
        {
            switch (update)
            {
                case { CallbackQuery: { } callbackQuery }: await OnCallbackQuery(callbackQuery); break;
                case { PollAnswer: { } pollAnswer }: await OnPollAnswer(pollAnswer); break;
                default: Console.WriteLine($"Получено необработанное обновление {update.Type}"); break;
            };
        }

        async Task OnCallbackQuery(CallbackQuery callbackQuery)
        {
            await bot.AnswerCallbackQuery(callbackQuery.Id, $"Вы выбрали {callbackQuery.Data}");
            await bot.SendMessage(callbackQuery.Message!.Chat, $"Получен обратный вызов от встроенной кнопки {callbackQuery.Data}");
        }

        async Task OnPollAnswer(PollAnswer pollAnswer)
        {
            if (pollAnswer.User != null)
                await bot.SendMessage(pollAnswer.User.Id, $"You voted for option(s) id [{string.Join(',', pollAnswer.OptionIds)}]");
        }

        async Task OnDocumentReceived(Message msg)
        {
            // Проверяем, ожидает ли пользователь загрузки файла
            if (userWaitingForFile.ContainsKey(msg.Chat.Id) && userWaitingForFile[msg.Chat.Id])
            {
                // Обрабатываем полученный документ
                await MessageHandler.HandleReceivedDocumentAsync(bot, msg);
                // Сбрасываем состояние ожидания
                userWaitingForFile[msg.Chat.Id] = false;
            }
            else
            {
                // Пользователь отправил документ, когда бот этого не ожидал
                await bot.SendTextMessageAsync(msg.Chat.Id, "Я не ожидаю от вас файл. Если вы хотите импортировать данные, нажмите кнопку '❓ Помощь'.");
            }
        }
    }
}