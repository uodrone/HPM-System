using HPM_System.NotificationService.Application.DTO;
using HPM_System.NotificationService.Application.Handlers;
using HPM_System.NotificationService.Application.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace HPM_System.NotificationService.Infrastructure.RabbitMQ;

public class RabbitUserConsumer : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _configuration;

    public RabbitUserConsumer(IServiceScopeFactory scopeFactory, IConfiguration configuration)
    {
        _scopeFactory = scopeFactory;
        _configuration = configuration;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _configuration["RabbitMQ:HostName"],
            Port = int.Parse(_configuration["RabbitMQ:Port"] ?? "5672"),
            DispatchConsumersAsync = true
        };

        var exchange = _configuration["RabbitMQ:Exchange"] ?? "notification_user";
        var queue = "notification.user";
        var routingKeyPattern = "notification.user.*";

        var connection = factory.CreateConnection();
        var channel = connection.CreateModel();
        
        channel.ExchangeDeclare(exchange: exchange, type: ExchangeType.Topic, durable: true);        
        channel.QueueDeclare(queue: queue, durable: true, exclusive: false, autoDelete: false);       
        channel.QueueBind(queue: queue, exchange: exchange, routingKey: routingKeyPattern);
        
        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.Received += async (sender, ea) =>
        {
            try
            {
                var routingKey = ea.RoutingKey;
                var json = Encoding.UTF8.GetString(ea.Body.ToArray());

                using var scope = _scopeFactory.CreateScope();

                var rabbitUserHandler = scope.ServiceProvider.GetRequiredService<IRabbitUserHandler>();
                await rabbitUserHandler.ExecuteAsync(new RabbitDTO { RoutingKey = routingKey, Payload = json });

                channel.BasicAck(ea.DeliveryTag, false);
            }
            catch (JsonException jsonEx)
            {
                Console.WriteLine($"[RabbitMQ Deserialization Error] {jsonEx.Message}");
                channel.BasicNack(ea.DeliveryTag, false, requeue: false);
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"[RabbitMQ Logic Error] {ex.Message}");
                channel.BasicNack(ea.DeliveryTag, false, requeue: false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RabbitMQ Consumer Error] {ex.Message}");
                channel.BasicNack(ea.DeliveryTag, false, requeue: true);
            }
        };
        
        channel.BasicConsume(queue: queue, autoAck: false, consumer: consumer);

        return Task.CompletedTask;
    }
}
