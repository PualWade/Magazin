using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Magazin.Handlers
{
    public class ChatMemberHandler
    {
        public async Task HandleChatMemberUpdatedAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var chatMemberUpdated = update.ChatMember ?? update.MyChatMember;

            // Обработка изменений статуса участника чата
            // Например, приветствие новых пользователей или прощание с уходящими
        }
    }
}
