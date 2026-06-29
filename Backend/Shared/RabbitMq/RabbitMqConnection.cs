using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;

namespace Shared.RabbitMq;

public class RabbitMqConnection : IHostedService, IDisposable
{
    private IConnection? _connection;
    private readonly string _hostName;

    public RabbitMqConnection(IConfiguration configuration)
    {
        _hostName = configuration["RabbitMQ:HostName"] ?? "localhost";
    }

    public IConnection Connection => _connection ?? throw new InvalidOperationException("RabbitMQ not connected");

    public async Task StartAsync(CancellationToken ct)
    {
        var factory = new ConnectionFactory
        {
            HostName = _hostName,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(5)
        };

        _connection = await factory.CreateConnectionAsync(ct);

        _connection.ConnectionShutdownAsync += (_, args) =>
        {
            Console.WriteLine($"[RabbitMQ] Connection shutdown: {args.ReplyText}");
            return Task.CompletedTask;
        };
    }

    public async Task StopAsync(CancellationToken ct)
    {
        if (_connection is not null)
            await _connection.CloseAsync(ct);
    }

    public void Dispose() => _connection?.Dispose();
}
