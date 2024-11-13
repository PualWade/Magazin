using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Magazin.Handlers;

namespace Magazin.Services
{
    public class BotManager
    {
        private readonly ITelegramBotClient _botClient;
        private readonly CancellationTokenSource _cts;
        private readonly UpdateHandler _updateHandler;

        public BotManager(string token)
        {
            _botClient = new TelegramBotClient(token);
            _cts = new CancellationTokenSource();
            _updateHandler = new UpdateHandler();

            // Регистрация сервисов можно добавить здесь при необходимости
        }

        public async Task StartAsync()
        {
            _botClient.StartReceiving(
                _updateHandler,
                receiverOptions: null,
                cancellationToken: _cts.Token
            );

            var me = await _botClient.GetMe();
            Console.WriteLine($"Бот @{me.Username} запущен.");
        }

        public Task StopAsync()
        {
            _cts.Cancel();
            return Task.CompletedTask;
        }
    }
}
