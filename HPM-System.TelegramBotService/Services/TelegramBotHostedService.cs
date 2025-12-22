using HPM_System.TelegramBotService.Data;
using HPM_System.TelegramBotService.Models;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace HPM_System.TelegramBotService.Services;

public class TelegramBotHostedService : BackgroundService
{
    private readonly ITelegramBotClient _botClient;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TelegramBotHostedService> _logger;

    public TelegramBotHostedService(
        ITelegramBotClient botClient,
        IServiceProvider serviceProvider,
        ILogger<TelegramBotHostedService> logger)
    {
        _botClient = botClient;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = new[] { UpdateType.Message, UpdateType.PollAnswer } // Убрали CallbackQuery
        };

        var updateHandler = new DefaultUpdateHandler(
            updateHandler: HandleUpdateAsync,
            errorHandler: HandleErrorAsync
        );

        var me = await _botClient.GetMe(stoppingToken);
        _logger.LogInformation("Telegram-бот @{BotUsername} запущен", me.Username);

        await _botClient.ReceiveAsync(
            updateHandler: updateHandler,
            receiverOptions: receiverOptions,
            cancellationToken: stoppingToken
        );
    }

    private async Task HandleUpdateAsync(
        ITelegramBotClient botClient,
        Update update,
        CancellationToken cancellationToken)
    {
        // Обработка ответов на Poll
        if (update.PollAnswer != null)
        {
            await HandlePollAnswerAsync(update.PollAnswer, cancellationToken);
            return;
        }

        if (update.Message is not { } message)
            return;

        var chatId = message.Chat.Id;

        // Обработка контакта
        if (message.Contact != null)
        {
            await HandleContactAsync(chatId, message.Contact, cancellationToken);
            return;
        }

        // Обработка текстовых команд
        if (message.Text is { } messageText)
        {
            _logger.LogInformation("Получено сообщение '{Text}' от chatId: {ChatId}", messageText, chatId);

            if (messageText.StartsWith("/start"))
            {
                await HandleStartCommand(chatId, cancellationToken);
            }
            else
            {
                await botClient.SendMessage(
                    chatId: chatId,
                    text: "Используйте команду /start для подписки на уведомления.",
                    cancellationToken: cancellationToken);
            }
        }
    }

    private async Task HandlePollAnswerAsync(PollAnswer pollAnswer, CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var votingClient = scope.ServiceProvider.GetRequiredService<VotingServiceClient>();

            // Получаем Telegram UserId из ответа
            var telegramUserId = pollAnswer.User.Id;
            var pollId = pollAnswer.PollId;

            _logger.LogInformation("Получен PollAnswer от Telegram User {TelegramUserId}, PollId: '{PollId}'",
                telegramUserId, pollId);

            // Ищем TelegramUser по Telegram ChatId
            var telegramUser = await context.TelegramUsers
                .FirstOrDefaultAsync(u => u.TelegramChatId == telegramUserId, cancellationToken);

            if (telegramUser == null)
            {
                _logger.LogWarning("TelegramUser не найден для Telegram UserId {TelegramUserId}", telegramUserId);
                return;
            }

            _logger.LogInformation("Найден TelegramUser: UserId={UserId}, ищем poll с PollId='{PollId}'",
                telegramUser.UserId, pollId);

            // Сначала пытаемся найти по PollId
            var telegramPoll = await context.TelegramPolls
                .Where(p => p.UserId == telegramUser.UserId && !p.IsAnswered)
                .ToListAsync(cancellationToken);

            _logger.LogInformation("Найдено неотвеченных polls для пользователя {UserId}: {Count}",
                telegramUser.UserId, telegramPoll.Count);

            if (telegramPoll.Any())
            {
                foreach (var p in telegramPoll)
                {
                    _logger.LogInformation("  Poll: VotingId={VotingId}, PollId='{PollId}', ApartmentId={ApartmentId}",
                        p.VotingId, p.PollId, p.ApartmentId);
                }
            }

            // Ищем конкретный poll по PollId
            var matchingPoll = telegramPoll.FirstOrDefault(p => p.PollId == pollId);

            if (matchingPoll == null)
            {
                _logger.LogWarning("TelegramPoll с PollId '{PollId}' не найден для пользователя {UserId}",
                    pollId, telegramUser.UserId);

                // Проверяем, есть ли уже отвеченный poll с таким PollId
                var answeredPoll = await context.TelegramPolls
                    .Where(p => p.UserId == telegramUser.UserId && p.IsAnswered && p.PollId == pollId)
                    .FirstOrDefaultAsync(cancellationToken);

                if (answeredPoll != null)
                {
                    await _botClient.SendMessage(
                        chatId: telegramUserId,
                        text: $"ℹ️ Вы уже проголосовали в этом голосовании.\n\n" +
                              $"Ваш выбор: <b>{answeredPoll.SelectedOption}</b>",
                        parseMode: ParseMode.Html,
                        cancellationToken: cancellationToken);
                }
                else
                {
                    _logger.LogError("Poll не найден ни среди активных, ни среди отвеченных. PollId: '{PollId}'", pollId);
                }
                return;
            }

            _logger.LogInformation("Найден TelegramPoll: VotingId={VotingId}, UserId={UserId}, PollId='{PollId}'",
                matchingPoll.VotingId, matchingPoll.UserId, matchingPoll.PollId);

            // Получаем выбранный вариант
            if (pollAnswer.OptionIds.Length == 0)
            {
                _logger.LogWarning("Пользователь не выбрал ни один вариант в poll");
                return;
            }

            var optionIndex = pollAnswer.OptionIds[0];

            // Получаем текст варианта ответа из VotingService
            var voting = await votingClient.GetVotingByIdAsync(matchingPoll.VotingId, cancellationToken);

            if (voting == null || optionIndex >= voting.ResponseOptions.Count)
            {
                _logger.LogError("Не удалось получить вариант ответа для голосования {VotingId}, optionIndex: {OptionIndex}",
                    matchingPoll.VotingId, optionIndex);
                return;
            }

            var selectedOption = voting.ResponseOptions[optionIndex];

            _logger.LogInformation("Пользователь {UserId} выбрал вариант: {SelectedOption} для голосования {VotingId}",
                matchingPoll.UserId, selectedOption, matchingPoll.VotingId);

            // Отправляем голос в VotingService (за ВСЕ квартиры пользователя)
            var result = await votingClient.SubmitVoteAsync(
                matchingPoll.VotingId,
                matchingPoll.UserId,
                selectedOption,
                cancellationToken);

            if (result.Success)
            {
                // Отмечаем ВСЕ polls этого пользователя в ЭТОМ КОНКРЕТНОМ голосовании как отвеченные
                var allUserPolls = await context.TelegramPolls
                    .Where(p => p.VotingId == matchingPoll.VotingId && p.UserId == matchingPoll.UserId)
                    .ToListAsync(cancellationToken);

                foreach (var p in allUserPolls)
                {
                    p.IsAnswered = true;
                    p.SelectedOption = selectedOption;
                }

                await context.SaveChangesAsync(cancellationToken);

                var message = allUserPolls.Count > 1
                    ? $"✅ Ваш голос принят: <b>{selectedOption}</b>\n\n" +
                      $"Учтены все ваши квартиры ({allUserPolls.Count} шт.) с суммарным весом голоса."
                    : $"✅ Ваш голос принят: <b>{selectedOption}</b>";

                await _botClient.SendMessage(
                    chatId: telegramUserId,
                    text: message,
                    parseMode: ParseMode.Html,
                    cancellationToken: cancellationToken);

                _logger.LogInformation("Голос пользователя {UserId} по голосованию {VotingId} успешно отправлен (квартир: {Count})",
                    matchingPoll.UserId, matchingPoll.VotingId, allUserPolls.Count);
            }
            else if (result.AlreadyVoted)
            {
                // Пользователь уже проголосовал через веб-интерфейс
                var allUserPolls = await context.TelegramPolls
                    .Where(p => p.VotingId == matchingPoll.VotingId && p.UserId == matchingPoll.UserId)
                    .ToListAsync(cancellationToken);

                foreach (var p in allUserPolls)
                {
                    p.IsAnswered = true;
                    p.SelectedOption = result.PreviousResponse;
                }

                await context.SaveChangesAsync(cancellationToken);

                await _botClient.SendMessage(
                    chatId: telegramUserId,
                    text: $"ℹ️ Вы уже проголосовали ранее через веб-интерфейс.\n\n" +
                          $"Ваш выбор: <b>{result.PreviousResponse}</b>\n\n" +
                          $"Изменить голос нельзя.",
                    parseMode: ParseMode.Html,
                    cancellationToken: cancellationToken);

                _logger.LogInformation("Пользователь {UserId} попытался проголосовать повторно в голосовании {VotingId}",
                    matchingPoll.UserId, matchingPoll.VotingId);
            }
            else
            {
                await _botClient.SendMessage(
                    chatId: telegramUserId,
                    text: $"❌ Ошибка при отправке голоса: {result.Message}\n\n" +
                          "Попробуйте проголосовать через веб-интерфейс.",
                    cancellationToken: cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обработке ответа на poll");
        }
    }

    private async Task HandleStartCommand(long chatId, CancellationToken cancellationToken)
    {
        try
        {
            var requestContact = new ReplyKeyboardMarkup(new[]
            {
                new KeyboardButton("📱 Поделиться контактом") { RequestContact = true }
            })
            {
                ResizeKeyboard = true,
                OneTimeKeyboard = true
            };

            await _botClient.SendMessage(
                chatId: chatId,
                text: "👋 Добро пожаловать!\n\n" +
                      "Для подписки на уведомления от системы, пожалуйста, поделитесь своим номером телефона.\n\n" +
                      "Нажмите на кнопку ниже ⬇️",
                replyMarkup: requestContact,
                cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обработке команды /start");
        }
    }

    private async Task HandleContactAsync(long chatId, Contact contact, CancellationToken cancellationToken)
    {
        try
        {
            var phoneNumber = contact.PhoneNumber;
            _logger.LogInformation("Получен контакт с номером {Phone} от chatId {ChatId}", phoneNumber, chatId);

            var removeKeyboard = new ReplyKeyboardRemove();

            using var scope = _serviceProvider.CreateScope();
            var userServiceClient = scope.ServiceProvider.GetRequiredService<UserServiceClient>();
            var userId = await userServiceClient.GetUserIdByPhoneNumberAsync(phoneNumber, cancellationToken);

            if (userId == null)
            {
                await _botClient.SendMessage(
                    chatId: chatId,
                    text: "❌ К сожалению, пользователь с таким номером телефона не найден в системе.\n\n" +
                          "Пожалуйста, убедитесь, что вы зарегистрированы в системе HPM.",
                    replyMarkup: removeKeyboard,
                    cancellationToken: cancellationToken);
                return;
            }

            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var existingUser = await context.TelegramUsers
                .FirstOrDefaultAsync(u => u.UserId == userId.Value, cancellationToken);

            if (existingUser != null)
            {
                existingUser.TelegramChatId = chatId;
                existingUser.SubscribedAt = DateTime.UtcNow;
                await context.SaveChangesAsync(cancellationToken);

                await _botClient.SendMessage(
                    chatId: chatId,
                    text: "✅ Ваша подписка обновлена!\n\n" +
                          "Вы будете получать уведомления от системы HPM.",
                    replyMarkup: removeKeyboard,
                    cancellationToken: cancellationToken);
            }
            else
            {
                var newUser = new TelegramUser
                {
                    UserId = userId.Value,
                    TelegramChatId = chatId,
                    SubscribedAt = DateTime.UtcNow
                };

                context.TelegramUsers.Add(newUser);
                await context.SaveChangesAsync(cancellationToken);

                await _botClient.SendMessage(
                    chatId: chatId,
                    text: "✅ Вы успешно подписались на уведомления!\n\n" +
                          "Теперь вы будете получать важные сообщения от системы HPM.",
                    replyMarkup: removeKeyboard,
                    cancellationToken: cancellationToken);
            }

            _logger.LogInformation("Пользователь {UserId} с номером {Phone} подписался с chatId {ChatId}",
                userId.Value, phoneNumber, chatId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обработке контакта");

            await _botClient.SendMessage(
                chatId: chatId,
                text: "❌ Произошла ошибка при подписке. Попробуйте позже.",
                replyMarkup: new ReplyKeyboardRemove(),
                cancellationToken: cancellationToken);
        }
    }

    private Task HandleErrorAsync(
        ITelegramBotClient botClient,
        Exception exception,
        HandleErrorSource source,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Ошибка в Telegram боте (источник: {Source})", source);
        return Task.CompletedTask;
    }
}