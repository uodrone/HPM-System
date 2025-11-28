using HPM_System.NotificationService.Application.DTO;
using HPM_System.NotificationService.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace HPM_System.NotificationService.Infrastructure.RabbitMQ;

public class RabbitMQConsumer : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private readonly ILogger<RabbitMQConsumer> _logger;

    public RabbitMQConsumer(
        IServiceProvider serviceProvider,
        IConfiguration configuration,
        ILogger<RabbitMQConsumer> logger)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _configuration["RabbitMQ:HostName"] ?? "localhost",
            Port = int.Parse(_configuration["RabbitMQ:Port"] ?? "5672"),
            UserName = _configuration["RabbitMQ:Username"] ?? "guest",
            Password = _configuration["RabbitMQ:Password"] ?? "guest"
        };

        var exchange = _configuration["RabbitMQ:Exchange"] ?? "notification_topic";

        // Retry логика для подключения к RabbitMQ
        const int maxRetries = 5;
        var retryCount = 0;
        IConnection? connection = null;
        IChannel? channel = null;

        while (retryCount < maxRetries && !stoppingToken.IsCancellationRequested)
        {
            try
            {
                connection = await factory.CreateConnectionAsync(stoppingToken);
                channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);
                _logger.LogInformation("Успешное подключение к RabbitMQ");
                break;
            }
            catch (Exception ex)
            {
                retryCount++;
                _logger.LogWarning(ex, "Попытка подключения к RabbitMQ {Attempt}/{MaxRetries} не удалась",
                    retryCount, maxRetries);

                if (retryCount >= maxRetries)
                {
                    _logger.LogError("Не удалось подключиться к RabbitMQ после {MaxRetries} попыток", maxRetries);
                    throw;
                }

                await Task.Delay(5000, stoppingToken);
            }
        }

        if (channel == null || connection == null)
        {
            _logger.LogError("Не удалось создать канал RabbitMQ");
            return;
        }

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var handlers = scope.ServiceProvider.GetServices<IRabbitMQHandler>();

            foreach (var handler in handlers)
            {
                // Объявляем exchange, очередь и binding
                await channel.ExchangeDeclareAsync(
                    exchange: exchange,
                    type: ExchangeType.Topic,
                    durable: true,
                    autoDelete: false,
                    cancellationToken: stoppingToken);

                await channel.QueueDeclareAsync(
                    queue: handler.QueueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    cancellationToken: stoppingToken);

                await channel.QueueBindAsync(
                    queue: handler.QueueName,
                    exchange: exchange,
                    routingKey: handler.RoutingKeyPattern,
                    cancellationToken: stoppingToken);

                var consumer = new AsyncEventingBasicConsumer(channel);

                consumer.ReceivedAsync += async (sender, ea) =>
                {
                    if (stoppingToken.IsCancellationRequested) return;

                    try
                    {
                        var routingKey = ea.RoutingKey;
                        var payload = Encoding.UTF8.GetString(ea.Body.ToArray());

                        _logger.LogInformation("Получено сообщение с routing key '{RoutingKey}' в очереди '{Queue}'",
                            routingKey, handler.QueueName);

                        // Создаём новый scope для каждого сообщения
                        using var messageScope = _serviceProvider.CreateScope();
                        var scopedHandler = messageScope.ServiceProvider
                            .GetServices<IRabbitMQHandler>()
                            .FirstOrDefault(h => h.QueueName == handler.QueueName);

                        if (scopedHandler != null)
                        {
                            await scopedHandler.ExecuteAsync(new RabbitMQDTO
                            {
                                RoutingKey = routingKey,
                                Payload = payload
                            });
                        }

                        await channel.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);
                        _logger.LogInformation("Сообщение успешно обработано");
                    }
                    catch (JsonException jsonEx)
                    {
                        _logger.LogError(jsonEx, "Ошибка десериализации сообщения RabbitMQ");
                        await channel.BasicNackAsync(ea.DeliveryTag, false, requeue: false, stoppingToken);
                    }
                    catch (InvalidOperationException ex)
                    {
                        _logger.LogError(ex, "Ошибка логики обработки RabbitMQ");
                        await channel.BasicNackAsync(ea.DeliveryTag, false, requeue: false, stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Ошибка обработки сообщения RabbitMQ");
                        await channel.BasicNackAsync(ea.DeliveryTag, false, requeue: true, stoppingToken);
                    }
                };

                await channel.BasicConsumeAsync(
                    queue: handler.QueueName,
                    autoAck: false,
                    consumer: consumer,
                    cancellationToken: stoppingToken);

                _logger.LogInformation("Подписка на очередь '{Queue}' с routing key '{RoutingKey}' активирована",
                    handler.QueueName, handler.RoutingKeyPattern);
            }

            // Ждём отмены
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("RabbitMQ Consumer остановлен");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Критическая ошибка в RabbitMQ Consumer");
        }
        finally
        {
            if (channel != null)
            {
                try
                {
                    await channel.CloseAsync(stoppingToken);
                }
                catch { }
            }

            if (connection != null)
            {
                try
                {
                    await connection.CloseAsync(stoppingToken);
                }
                catch { }
            }
        }
    }
}