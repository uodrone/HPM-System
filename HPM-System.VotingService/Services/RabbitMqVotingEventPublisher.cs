using Microsoft.AspNetCore.Connections;
using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

namespace VotingService.Services;

public class RabbitMqVotingEventPublisher : IVotingEventPublisher, IDisposable
{
    private readonly IConnection _connection;
    private readonly IChannel _channel;
    private readonly ILogger<RabbitMqVotingEventPublisher> _logger;

    public RabbitMqVotingEventPublisher(IConfiguration config, ILogger<RabbitMqVotingEventPublisher> logger)
    {
        _logger = logger;

        var factory = new ConnectionFactory
        {
            HostName = config["RabbitMQ:HostName"] ?? "localhost",
            Port = int.Parse(config["RabbitMQ:Port"] ?? "5672"),
            UserName = config["RabbitMQ:Username"] ?? "guest",
            Password = config["RabbitMQ:Password"] ?? "guest"
        };

        _connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
        _channel = _connection.CreateChannelAsync().GetAwaiter().GetResult();

        const string exchange = "votings";
        _channel.ExchangeDeclareAsync(exchange, ExchangeType.Topic, durable: true, autoDelete: false)
            .GetAwaiter().GetResult();
    }

    public async Task PublishVotingCreatedAsync(
        Guid votingId,
        string questionPut,
        List<string> responseOptions,
        DateTime endTime,
        List<(Guid UserId, long ApartmentId)> participants)
    {
        try
        {
            var eventData = new
            {
                votingId,
                questionPut,
                responseOptions,
                endTime,
                participants = participants.Select(p => new { userId = p.UserId, apartmentId = p.ApartmentId }).ToList()
            };

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var json = JsonSerializer.Serialize(eventData, options);
            var body = Encoding.UTF8.GetBytes(json);

            await _channel.BasicPublishAsync(
                exchange: "votings",
                routingKey: "voting.created",
                body: body);

            _logger.LogInformation("Опубликовано событие voting.created для голосования {VotingId}", votingId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка публикации события voting.created для голосования {VotingId}", votingId);
            throw;
        }
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
}