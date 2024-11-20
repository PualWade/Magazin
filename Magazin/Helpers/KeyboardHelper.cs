using System.Collections.Generic;
using Telegram.Bot.Types.ReplyMarkups;
using Magazin.Models;
using Telegram.Bot.Types.InlineQueryResults;

namespace Magazin.Helpers
{
    using Telegram.Bot.Types.ReplyMarkups;

    public static class KeyboardHelper
    {
        public static ReplyKeyboardMarkup GetMainMenuKeyboard()
        {
            return new ReplyKeyboardMarkup(new[]
            {
                new KeyboardButton[] { "🍴 Каталог", "📦 Заказы", "🛍 Корзина" },
                new KeyboardButton[] { "⚙️ Настройки", "❓ Помощь" }
            })
            {
                ResizeKeyboard = true,  // Уменьшает размери клавиатури
                OneTimeKeyboard = false // Клавиатура всегда видна 
            };
        }

        public static ReplyKeyboardMarkup GetCancelKeyboard()
        {
            return new ReplyKeyboardMarkup(new[]
            {
                new KeyboardButton("🚫 Отмена")
            })
            {
                ResizeKeyboard = true
            };
        }




        public static InlineKeyboardMarkup GetCategoriesInlineKeyboard(List<Category> categories)
        {
            var inlineKeyboardButtons = new List<List<InlineKeyboardButton>>();

            foreach (var category in categories)
            {
                var button = InlineKeyboardButton.WithCallbackData(category.CategoryName, $"category_{category.CategoryId}");
                inlineKeyboardButtons.Add(new List<InlineKeyboardButton> { button });
            }

            // Добавляем кнопку "Отмена"
            var cancelButton = InlineKeyboardButton.WithCallbackData("Отмена", "cancel");
            inlineKeyboardButtons.Add(new List<InlineKeyboardButton> { cancelButton });

            return new InlineKeyboardMarkup(inlineKeyboardButtons);
        }
    }
}
