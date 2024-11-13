using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Magazin.Handlers
{
    public class CallbackQueryHandler
    {
        public async Task HandleCallbackQueryAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            await botClient.AnswerCallbackQuery(
                callbackQueryId: callbackQuery.Id,
                text: $"Вы выбрали: {callbackQuery.Data}",
                cancellationToken: cancellationToken
            );

            // Обработка логики в зависимости от callbackQuery.Data
            // Например, обновление сообщения, выполнение действия и т.д.
        }
    }
}
