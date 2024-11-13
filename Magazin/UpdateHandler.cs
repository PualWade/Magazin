using Magazin.Handlers;
using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Magazin.Handlers;
using DocumentFormat.OpenXml.Bibliography;

namespace Magazin
{
    public class UpdateHandler : IUpdateHandler
    {
        private readonly MessageHandler _messageHandler;
        private readonly CallbackQueryHandler _callbackQueryHandler;
        private readonly InlineQueryHandler _inlineQueryHandler;
        private readonly ChatMemberHandler _chatMemberHandler;

        public UpdateHandler()
        {
            _messageHandler = new MessageHandler();
            _callbackQueryHandler = new CallbackQueryHandler();
            _inlineQueryHandler = new InlineQueryHandler();
            _chatMemberHandler = new ChatMemberHandler();
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                switch (update.Type)
                {
                    case UpdateType.Message:
                        await _messageHandler.HandleMessageAsync(botClient, update.Message, cancellationToken);
                        break;
                    case UpdateType.CallbackQuery:
                        await _callbackQueryHandler.HandleCallbackQueryAsync(botClient, update.CallbackQuery, cancellationToken);
                        break;
                    case UpdateType.InlineQuery:
                        await _inlineQueryHandler.HandleInlineQueryAsync(botClient, update.InlineQuery, cancellationToken);
                        break;
                    case UpdateType.ChatMember:
                    case UpdateType.MyChatMember:
                        await _chatMemberHandler.HandleChatMemberUpdatedAsync(botClient, update, cancellationToken);
                        break;
                    // Добавьте обработку других типов обновлений при необходимости
                    default:
                        Console.WriteLine($"Необработанный тип обновления: {update.Type}");
                        break;
                }
            }
            catch (Exception ex)
            {
                await HandleErrorAsync(botClient, ex, HandleErrorSource.HandleUpdateError, cancellationToken);
            }
        }

        public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Ошибка: {exception.Message}");
            return Task.CompletedTask;
        }

        public UpdateType[] AllowedUpdates => Array.Empty<UpdateType>();
    }
}
