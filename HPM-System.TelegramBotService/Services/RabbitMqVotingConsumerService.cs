using HPM_System.TelegramBotService.Data;
using HPM_System.TelegramBotService.DTO;
using HPM_System.TelegramBotService.Models;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace HPM_System.TelegramBotService.Services;

public class RabbitMqVotingConsumerService : BackgroundService
{
    private readonly IConfiguration _config;
    private readonly ILogger<RabbitMqVotingConsumerService> _logger;
    private readonly ITelegramBotClient _botClient;
    private readonly IServiceProvider _serviceProvider;

    public RabbitMqVotingConsumerService(
        IConfiguration config,
        ILogger<RabbitMqVotingConsumerService> logger,
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

        const string ex = "votings";
        const string q = "telegram-votings-queue";
        const string rk = "voting.created";

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
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    PropertyNameCaseInsensitive = true
                };

                var votingEvent = JsonSerializer.Deserialize<VotingCreatedEvent>(json, options);

                if (votingEvent == null)
                {
                    _logger.LogWarning("Не удалось десериализовать событие голосования");
                    await channel.BasicNackAsync(args.DeliveryTag, false, requeue: false, ct);
                    return;
                }

                await ProcessVotingEventAsync(votingEvent, ct);
                await channel.BasicAckAsync(args.DeliveryTag, false, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка обработки события голосования");
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
        _logger.LogInformation("Telegram-бот слушает очередь голосований '{Queue}'", q);

        try
        {
            await Task.Delay(Timeout.Infinite, ct);
        }
        catch (TaskCanceledException)
        {
            _logger.LogInformation("Служба Voting Consumer остановлена");
        }
        finally
        {
            await channel.CloseAsync(ct);
            await connection.CloseAsync(ct);
        }
    }

    private async Task ProcessVotingEventAsync(VotingCreatedEvent votingEvent, CancellationToken ct)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Группируем участников по UserId, чтобы отправить Poll только один раз каждому пользователю
        var uniqueUsers = votingEvent.Participants
            .GroupBy(p => p.UserId)
            .Select(g => new
            {
                UserId = g.Key,
                Apartments = g.Select(p => p.ApartmentId).ToList()
            })
            .ToList();

        foreach (var user in uniqueUsers)
        {
            try
            {
                var telegramUser = await context.TelegramUsers
                    .FirstOrDefaultAsync(u => u.UserId == user.UserId, ct);

                if (telegramUser == null)
                {
                    _logger.LogWarning("Пользователь {UserId} не найден в Telegram. Пропускаем отправку голосования.",
                        user.UserId);
                    continue;
                }

                // Создаем Poll в Telegram
                var pollOptions = votingEvent.ResponseOptions
                    .Select(opt => new InputPollOption(opt))
                    .ToList();

                var pollMessage = await _botClient.SendPoll(
                    chatId: telegramUser.TelegramChatId,
                    question: votingEvent.QuestionPut,
                    options: pollOptions,
                    isAnonymous: false,
                    allowsMultipleAnswers: false,
                    closeDate: votingEvent.EndTime,
                    cancellationToken: ct);

                // Сохраняем информацию о poll для каждой квартиры пользователя
                foreach (var apartmentId in user.Apartments)
                {
                    var telegramPoll = new TelegramPoll
                    {
                        VotingId = votingEvent.VotingId,
                        UserId = user.UserId,
                        ApartmentId = apartmentId,
                        PollId = pollMessage.Poll!.Id,
                        ChatId = telegramUser.TelegramChatId,
                        MessageId = pollMessage.MessageId,
                        IsAnswered = false
                    };

                    context.TelegramPolls.Add(telegramPoll);
                }

                await context.SaveChangesAsync(ct);

                _logger.LogInformation("Poll отправлен пользователю {UserId} для голосования {VotingId} (квартир: {ApartmentCount})",
                    user.UserId, votingEvent.VotingId, user.Apartments.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка отправки poll пользователю {UserId} для голосования {VotingId}",
                    user.UserId, votingEvent.VotingId);
            }
        }
    }
}