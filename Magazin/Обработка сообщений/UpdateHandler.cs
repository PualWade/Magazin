using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types.Enums;

public class UpdateHandler : IUpdateHandler
{
    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        // Обработка входящих сообщений
        if (update.Message != null)
        {
            // Входящее сообщение от пользователя
            // Пример: пользователь отправил текстовое сообщение или медиа
            await Program.ProcessMessageAsync(botClient, update.Message, cancellationToken);
        }
        // Обработка редактированных сообщений
        else if (update.EditedMessage != null)
        {
            // Пользователь отредактировал ранее отправленное сообщение
            // Пример: пользователь исправил опечатку в своём сообщении
            await Program.ProcessEditedMessageAsync(botClient, update.EditedMessage, cancellationToken);
        }
        // Обработка сообщений от канала
        else if (update.ChannelPost != null)
        {
            // Сообщение, отправленное в канал
            // Пример: бот получает сообщение, опубликованное в канале
            await Program.ProcessChannelPostAsync(botClient, update.ChannelPost, cancellationToken);
        }
        // Обработка редактированных сообщений в канале
        else if (update.EditedChannelPost != null)
        {
            // Редактированное сообщение в канале
            // Пример: сообщение в канале было отредактировано
            await Program.ProcessEditedChannelPostAsync(botClient, update.EditedChannelPost, cancellationToken);
        }
        // Обработка CallbackQuery
        else if (update.CallbackQuery != null)
        {
            // Пользователь нажал на кнопку с callback data
            // Пример: inline-кнопка с данными обратного вызова
            await Program.HandleCallbackQueryAsync(botClient, update.CallbackQuery, cancellationToken);
        }
        // Обработка InlineQuery
        else if (update.InlineQuery != null)
        {
            // Пользователь использует бота в inline-режиме
            // Пример: пользователь вводит @your_bot в поле ввода и набирает запрос
            await Program.HandleInlineQueryAsync(botClient, update.InlineQuery, cancellationToken);
        }
        // Обработка выбранного результата инлайн-запроса
        else if (update.ChosenInlineResult != null)
        {
            // Пользователь выбрал один из результатов inline-запроса
            // Пример: пользователь выбрал результат, предоставленный вашим ботом
            await Program.HandleChosenInlineResultAsync(botClient, update.ChosenInlineResult, cancellationToken);
        }
        // Обработка запросов о доставке (ShippingQuery)
        else if (update.ShippingQuery != null)
        {
            // Используется в платежах с параметром доставки
            // Пример: пользователь ввёл адрес доставки при покупке
            await Program.HandleShippingQueryAsync(botClient, update.ShippingQuery, cancellationToken);
        }
        // Обработка предварительных запросов на оплату (PreCheckoutQuery)
        else if (update.PreCheckoutQuery != null)
        {
            // Перед подтверждением оплаты
            // Пример: бот должен подтвердить, что готов обработать оплату
            await Program.HandlePreCheckoutQueryAsync(botClient, update.PreCheckoutQuery, cancellationToken);
        }
        // Обработка обновлений опросов (Poll)
        else if (update.Poll != null)
        {
            // Изменения в активных опросах
            // Пример: опрос был остановлен или обновлён
            await Program.HandlePollAsync(botClient, update.Poll, cancellationToken);
        }
        // Обработка ответов на опросы (PollAnswer)
        else if (update.PollAnswer != null)
        {
            // Пользователь проголосовал в опросе
            // Пример: пользователь выбрал вариант в опросе
            await Program.HandlePollAnswerAsync(botClient, update.PollAnswer, cancellationToken);
        }
        // Обработка обновлений статуса бота в чате (MyChatMember)
        else if (update.MyChatMember != null)
        {
            // Изменение статуса бота в чате
            // Пример: бот был добавлен или удалён из группы
            await Program.HandleMyChatMemberUpdatedAsync(botClient, update.MyChatMember, cancellationToken);
        }
        // Обработка обновлений статуса участника чата (ChatMember)
        else if (update.ChatMember != null)
        {
            // Изменение статуса пользователя в чате
            // Пример: пользователь был забанен или получил права администратора
            await Program.HandleChatMemberUpdatedAsync(botClient, update.ChatMember, cancellationToken);
        }
        // Обработка запросов на присоединение к чату (ChatJoinRequest)
        else if (update.ChatJoinRequest != null)
        {
            // Пользователь запросил доступ к приватной группе или каналу
            // Пример: бот может одобрить или отклонить запрос
            await Program.HandleChatJoinRequestAsync(botClient, update.ChatJoinRequest, cancellationToken);
        }
        else
        {
            // Обработка других типов обновлений
            Console.WriteLine($"Необработанный тип обновления: {update.Type}");
        }
    }


    public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source, CancellationToken cancellationToken)
    {
        // Логирование ошибки
        Console.WriteLine($"Ошибка: {exception.Message}");

        // Дополнительная обработка в зависимости от источника ошибки
        switch (exception)
        {
            case ApiRequestException apiRequestException:
                // Обработка ошибок API Telegram
                Console.WriteLine($"Ошибка Telegram API:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}");
                break;
            default:
                // Обработка других ошибок
                Console.WriteLine($"Неизвестная ошибка: {exception}");
                break;
        }

        return Task.CompletedTask;
    }

    public UpdateType[] AllowedUpdates => Array.Empty<UpdateType>(); // Разрешаем все типы обновлений
}
