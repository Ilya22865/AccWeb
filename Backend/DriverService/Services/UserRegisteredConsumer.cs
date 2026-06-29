using System.Text;
using System.Text.Json;
using DriverService.Data;
using DriverService.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared.Events;
using Shared.RabbitMq;

namespace DriverService.Services;

public class UserRegisteredConsumer : BackgroundService
{
    private readonly RabbitMqConnection _mq;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<UserRegisteredConsumer> _logger;

    public UserRegisteredConsumer(
        RabbitMqConnection mq,
        IServiceScopeFactory scopeFactory,
        ILogger<UserRegisteredConsumer> logger)
    {
        _mq = mq;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var channel = await _mq.Connection.CreateChannelAsync(cancellationToken: stoppingToken);
        await channel.ExchangeDeclareAsync("user.events", ExchangeType.Topic, durable: true, cancellationToken: stoppingToken);

        var queueName = (await channel.QueueDeclareAsync(
            queue: "driver-service.user-registered",
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: stoppingToken
        )).QueueName;

        await channel.QueueBindAsync(queueName, "user.events", "user.registered", cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (_, ea) =>
        {
            try
            {
                var body = Encoding.UTF8.GetString(ea.Body.ToArray());
                var @event = JsonSerializer.Deserialize<UserRegisteredEvent>(body)
                    ?? throw new Exception("Failed to deserialize event");

                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                db.Drivers.Add(new Driver
                {
                    AuthUserId = @event.AuthUserId,
                    FullName = @event.FullName
                });
                await db.SaveChangesAsync(stoppingToken);

                await channel.BasicAckAsync(ea.DeliveryTag, multiple: false, cancellationToken: stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process UserRegistered event");
                await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false, cancellationToken: stoppingToken);
            }
        };

        await channel.BasicConsumeAsync(queueName, autoAck: false, consumer: consumer, cancellationToken: stoppingToken);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}
