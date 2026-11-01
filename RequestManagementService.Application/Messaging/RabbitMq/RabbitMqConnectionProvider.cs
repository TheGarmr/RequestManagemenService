using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RequestManagementService.Application.Messaging.Options;

namespace RequestManagementService.Application.Messaging.RabbitMq;

internal sealed class RabbitMqConnectionProvider : IAsyncDisposable
{
    private readonly IOptions<RabbitMqOptions> _options;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private IConnection? _connection;

    public RabbitMqConnectionProvider(IOptions<RabbitMqOptions> options)
    {
        _options = options;
    }

    public async Task<IConnection> GetConnectionAsync(CancellationToken cancellationToken)
    {
        if (_connection is not null)
        {
            return _connection;
        }

        await _lock.WaitAsync(cancellationToken);

        try
        {
            if (_connection is null)
            {
                var factory = new ConnectionFactory
                {
                    HostName = _options.Value.HostName,
                    Port = _options.Value.Port,
                    UserName = _options.Value.UserName,
                    Password = _options.Value.Password,
                    VirtualHost = _options.Value.VirtualHost,
                    AutomaticRecoveryEnabled = true,
                    ClientProvidedName = "request-management-service"
                };

                _connection = await factory.CreateConnectionAsync(cancellationToken);
            }

            return _connection;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection is not null)
        {
            await _connection.DisposeAsync();
        }

        _lock.Dispose();
    }
}
