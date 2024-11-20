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
            var data = callbackQuery.Data;

            if (data.StartsWith("category_"))
            {
                // Извлекаем идентификатор категории
                var categoryIdString = data.Substring("category_".Length);
                if (int.TryParse(categoryIdString, out int categoryId))
                {
                    // Обрабатываем выбор категории
                    await HandleCategorySelectionAsync(botClient, callbackQuery, categoryId, cancellationToken);
                }
                else
                {
                    await botClient.AnswerCallbackQuery(
                        callbackQueryId: callbackQuery.Id,
                        text: "Некорректный идентификатор категории.",
                        showAlert: true,
                        cancellationToken: cancellationToken
                    );
                }
            }
            else if (data == "cancel")
            {
                // Обработка отмены
                await botClient.SendMessage(
                    chatId: callbackQuery.Message.Chat.Id,
                    text: "Операция отменена.",
                    cancellationToken: cancellationToken
                );
            }
            else
            {
                await botClient.AnswerCallbackQuery(
                    callbackQueryId: callbackQuery.Id,
                    text: "Неизвестная команда.",
                    showAlert: true,
                    cancellationToken: cancellationToken
                );
            }
        }

        private async Task HandleCategorySelectionAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery, int categoryId, CancellationToken cancellationToken)
        {
            // Заглушка для обработки выбора категории
            await botClient.SendMessage(
                chatId: callbackQuery.Message.Chat.Id,
                text: $"Вы выбрали категорию с ID {categoryId}. Функциональность будет реализована позже.",
                cancellationToken: cancellationToken
            );
        }


        
    }
}
