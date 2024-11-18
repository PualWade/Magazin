namespace Magazin.Helpers
{
    using Telegram.Bot.Types.ReplyMarkups;

    public static class KeyboardHelper
    {
        public static ReplyKeyboardMarkup GetMainMenuKeyboard()
        {
            return new ReplyKeyboardMarkup(new[]
            {
                new KeyboardButton[] { "🍴 Меню", "📦 Заказы", "🛍 Корзина" },
                new KeyboardButton[] { "⚙️ Настройки", "❓ Помощь" }
            })
            {
                ResizeKeyboard = true
            };
        }

        public static ReplyKeyboardMarkup GetCancelKeyboard()
        {
            return new ReplyKeyboardMarkup(new[]
            {
                new KeyboardButton("Отмена")
            })
            {
                ResizeKeyboard = true
            };
        }
    }
}
