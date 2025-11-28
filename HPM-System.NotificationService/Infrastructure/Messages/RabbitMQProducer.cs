using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace HPM_System.NotificationService.Infrastructure.RabbitMQ;

public class RabbitMQProducer : IDisposable
{
    private IConnection? _connection;
    private IChannel? _channel;
    private readonly ILogger<RabbitMQProducer> _logger;
    private readonly ConnectionFactory _factory;
    private readonly string _exchange;
    private readonly SemaphoreSlim _connectionLock = new(1, 1);
    private bool _disposed = false;

    public RabbitMQProducer(IConfiguration configuration, ILogger<RabbitMQProducer> logger)
    {
        _logger = logger;
        _exchange = configuration["RabbitMQ:Exchange"] ?? "notifications";

        _factory = new ConnectionFactory
        {
            HostName = configuration["RabbitMQ:HostName"] ?? "localhost",
            Port = int.Parse(configuration["RabbitMQ:Port"] ?? "5672"),
            UserName = configuration["RabbitMQ:Username"] ?? "guest",
            Password = configuration["RabbitMQ:Password"] ?? "guest",
            AutomaticRecoveryEnabled = true,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
        };

        // Инициализируем подключение при создании
        EnsureConnectionAsync(CancellationToken.None).GetAwaiter().GetResult();
    }

    private async Task EnsureConnectionAsync(CancellationToken cancellationToken)
    {
        if (_connection != null && _connection.IsOpen && _channel != null && _channel.IsOpen)
        {
            return;
        }

        await _connectionLock.WaitAsync(cancellationToken);
        try
        {
            // Двойная проверка после получения блокировки
            if (_connection != null && _connection.IsOpen && _channel != null && _channel.IsOpen)
            {
                return;
            }

            _logger.LogInformation("Создание подключения к RabbitMQ...");

            // Закрываем старые подключения, если есть
            if (_channel != null)
            {
                try { await _channel.CloseAsync(cancellationToken); } catch { }
                _channel?.Dispose();
            }

            if (_connection != null)
            {
                try { await _connection.CloseAsync(cancellationToken); } catch { }
                _connection?.Dispose();
            }

            // Создаём новые
            _connection = await _factory.CreateConnectionAsync(cancellationToken);
            _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

            // Объявляем exchange
            await _channel.ExchangeDeclareAsync(
                exchange: _exchange,
                type: ExchangeType.Topic,
                durable: true,
                autoDelete: false,
                cancellationToken: cancellationToken
            );

            _logger.LogInformation("RabbitMQ Producer успешно подключен к exchange '{Exchange}'", _exchange);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при создании подключения к RabbitMQ");
            throw;
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    public async Task PublishAsync<T>(string routingKey, T message, CancellationToken cancellationToken = default)
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(RabbitMQProducer));
        }

        // Пытаемся переподключиться, если соединение упало
        await EnsureConnectionAsync(cancellationToken);

        if (_channel == null)
        {
            throw new InvalidOperationException("RabbitMQ канал не инициализирован");
        }

        try
        {
            var json = JsonSerializer.Serialize(message, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            var body = Encoding.UTF8.GetBytes(json);

            var properties = new BasicProperties
            {
                Persistent = true,
                ContentType = "application/json",
                DeliveryMode = DeliveryModes.Persistent,
                Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds())
            };

            await _channel.BasicPublishAsync(
                exchange: _exchange,
                routingKey: routingKey,
                mandatory: false,
                basicProperties: properties,
                body: body,
                cancellationToken: cancellationToken
            );

            _logger.LogInformation(
                "Сообщение опубликовано в exchange '{Exchange}' с routing key '{RoutingKey}'",
                _exchange, routingKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при публикации сообщения в RabbitMQ");

            // Сбрасываем подключение для повторной попытки в следующий раз
            _connection = null;
            _channel = null;

            throw;
        }
    }

    public void Dispose()
    {
        if (_disposed) return;

        try
        {
            _channel?.CloseAsync().GetAwaiter().GetResult();
            _channel?.Dispose();
            _connection?.CloseAsync().GetAwaiter().GetResult();
            _connection?.Dispose();
            _connectionLock?.Dispose();
            _logger.LogInformation("RabbitMQ Producer закрыт");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при закрытии RabbitMQ Producer");
        }
        finally
        {
            _disposed = true;
        }
    }
}