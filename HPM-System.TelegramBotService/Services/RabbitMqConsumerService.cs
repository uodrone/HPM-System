using HPM_System.TelegramBotService.DTO;
using HPM_System.TelegramBotService.Interfaces;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace HPM_System.TelegramBotService.Services;

public class RabbitMqConsumerService : BackgroundService
{
    private readonly IConfiguration _config;
    private readonly ILogger<RabbitMqConsumerService> _logger;
    private readonly ITelegramBotClient _botClient;
    private readonly IServiceProvider _serviceProvider;

    public RabbitMqConsumerService(
        IConfiguration config,
        ILogger<RabbitMqConsumerService> logger,
        ITelegramBotClient botClient,
        IServiceProvider serviceProvider)
    {
        _config = config;
        _logger = logger;
        _botClient = botClient;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        var factory = new ConnectionFactory
        {
            HostName = _config["RabbitMQ:Host"] ?? "localhost",
            Port = int.Parse(_config["RabbitMQ:Port"] ?? "5672"),
            UserName = _config["RabbitMQ:Username"] ?? "guest",
            Password = _config["RabbitMQ:Password"] ?? "guest"
        };

        var connection = await factory.CreateConnectionAsync(ct);
        var channel = await connection.CreateChannelAsync(cancellationToken: ct);

        const string ex = "notifications";
        const string q = "telegram-notifications-queue";
        const string rk = "notification.created";

        await channel.ExchangeDeclareAsync(ex, ExchangeType.Topic, durable: true, autoDelete: false, cancellationToken: ct);
        await channel.QueueDeclareAsync(q, durable: true, exclusive: false, autoDelete: false, cancellationToken: ct);
        await channel.QueueBindAsync(q, ex, rk, cancellationToken: ct);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (sender, args) =>
        {
            if (ct.IsCancellationRequested) return;

            try
            {
                var json = Encoding.UTF8.GetString(args.Body.ToArray());
                var evt = JsonSerializer.Deserialize<NotificationPublishedEvent>(json);

                if (evt == null)
                {
                    _logger.LogWarning("Не удалось десериализовать сообщение");
                    await channel.BasicNackAsync(args.DeliveryTag, false, requeue: false, ct);
                    return;
                }

                // Создаём scope для получения scoped-сервиса
                using var scope = _serviceProvider.CreateScope();
                var userService = scope.ServiceProvider.GetRequiredService<IDbTelegramUserService>();

                foreach (var userId in evt.RecipientUserIds)
                {
                    try
                    {
                        var chatId = await userService.GetTelegramChatIdByUserIdAsync(userId);
                        if (chatId == null)
                        {
                            _logger.LogWarning("Не найден Telegram ChatId для пользователя {UserId}", userId);
                            continue;
                        }

                        var msg = $"<b>{evt.Title}</b>\n{evt.Message}";

                        if (!string.IsNullOrEmpty(evt.ImageUrl))
                        {
                            await _botClient.SendPhoto(
                                chatId: new ChatId(chatId.Value),
                                photo: InputFile.FromUri(evt.ImageUrl),
                                caption: msg,
                                parseMode: ParseMode.Html,
                                cancellationToken: ct);
                        }
                        else
                        {
                            await _botClient.SendMessage(
                                chatId: new ChatId(chatId.Value),
                                text: msg,
                                parseMode: ParseMode.Html,
                                cancellationToken: ct);
                        }

                        _logger.LogInformation("Сообщение отправлено пользователю {UserId}", userId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Ошибка отправки сообщения пользователю {UserId}", userId);
                    }
                }

                await channel.BasicAckAsync(args.DeliveryTag, false, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка обработки сообщения из RabbitMQ");
                try
                {
                    await channel.BasicNackAsync(args.DeliveryTag, false, requeue: false, ct);
                }
                catch (Exception nackEx)
                {
                    _logger.LogError(nackEx, "Ошибка при отправке Nack");
                }
            }
        };

        await channel.BasicConsumeAsync(q, autoAck: false, consumer: consumer, cancellationToken: ct);

        _logger.LogInformation("Telegram-бот слушает очередь '{Queue}'", q);

        // Ждём отмены
        try
        {
            await Task.Delay(Timeout.Infinite, ct);
        }
        catch (TaskCanceledException)
        {
            _logger.LogInformation("Служба RabbitMQ Consumer остановлена");
        }
        finally
        {
            await channel.CloseAsync(ct);
            await connection.CloseAsync(ct);
        }
    }
}