using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using Shared.RabbitMq;
using Shared.Events;

namespace AuthService.Services;

public class EventPublisher : IEventPublisher
{
    private readonly IChannel _channel;

    public EventPublisher(RabbitMqConnection mq)
    {
        _channel = mq.Connection.CreateChannelAsync().GetAwaiter().GetResult();
        _channel.ExchangeDeclareAsync("user.events", ExchangeType.Topic, durable: true).GetAwaiter().GetResult();
    }

    public async Task PublishUserRegisteredAsync(UserRegisteredEvent @event)
    {
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(@event));

        var props = new BasicProperties
        {
            Persistent = true,
            ContentType = "application/json"
        };

        await _channel.BasicPublishAsync(
            exchange: "user.events",
            routingKey: "user.registered",
            mandatory: true,
            basicProperties: props,
            body: body
        );
    }
}
