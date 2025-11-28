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
            AllowedUpdates = new[] { UpdateType.Message }
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

    private async Task HandleStartCommand(long chatId, CancellationToken cancellationToken)
    {
        try
        {
            // Создаём кнопку для отправки контакта
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

            // Убираем клавиатуру
            var removeKeyboard = new ReplyKeyboardRemove();

            // Получаем userId из UserService
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

            // Сохраняем связь userId и chatId в базе
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