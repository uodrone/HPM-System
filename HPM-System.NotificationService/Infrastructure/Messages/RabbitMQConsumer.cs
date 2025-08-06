using HPM_System.NotificationService.Application.DTO;
using HPM_System.NotificationService.Application.Handlers;
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

    public RabbitMQConsumer(IServiceProvider serviceProvider, IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
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

        var exchange = _configuration["RabbitMQ:Exchange"] ?? "notification_topic";
        var connection = factory.CreateConnection();
        var channel = connection.CreateModel();

        using var scope = _serviceProvider.CreateScope();
        var handlers = scope.ServiceProvider.GetServices<IRabbitMQHandler>();

        foreach (var handler in handlers)
        {
            channel.ExchangeDeclare(exchange, ExchangeType.Topic, durable: true);

            channel.QueueDeclare(handler.QueueName, durable: true, exclusive: false, autoDelete: false);
            channel.QueueBind(handler.QueueName, exchange, routingKey: handler.RoutingKeyPattern);

            var consumer = new AsyncEventingBasicConsumer(channel);

            consumer.Received += async (sender, ea) =>
            {
                try
                {
                    var routingKey = ea.RoutingKey;
                    var payload = Encoding.UTF8.GetString(ea.Body.ToArray());

                    await handler.ExecuteAsync(new RabbitMQDTO { RoutingKey = routingKey, Payload = payload });
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

            channel.BasicConsume(handler.QueueName, autoAck: false, consumer);
        }        

        return Task.CompletedTask;
    }
}
