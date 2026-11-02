using RabbitMQ.Client;
using RequestManagementService.Application.Messaging.Contracts;
using RequestManagementService.Application.Messaging.Outbox;
using RequestManagementService.DataAccess.Entities;

namespace RequestManagementService.Application.Messaging.RabbitMq;

public sealed class RabbitMqOutboxMessagePublisher : IOutboxMessagePublisher, IAsyncDisposable
{
    private static readonly CreateChannelOptions ChannelOptions = new(
        publisherConfirmationsEnabled: true,
        publisherConfirmationTrackingEnabled: true);

    private readonly RabbitMqConnectionProvider _connectionProvider;
    private readonly RabbitMqTopology _topology;
    private readonly MessageSerializer _serializer;
    private IChannel? _channel;

    public RabbitMqOutboxMessagePublisher(
        RabbitMqConnectionProvider connectionProvider,
        RabbitMqTopology topology,
        MessageSerializer serializer)
    {
        _connectionProvider = connectionProvider;
        _topology = topology;
        _serializer = serializer;
    }

    public async Task PublishAsync(OutboxMessageEntity message, CancellationToken cancellationToken)
    {
        var channel = await GetChannelAsync(cancellationToken);

        var body = _serializer.Serialize(ToContract(message));
        var properties = new BasicProperties
        {
            MessageId = message.Id.ToString(),
            ContentType = "application/json",
            DeliveryMode = DeliveryModes.Persistent,
            Timestamp = new AmqpTimestamp(new DateTimeOffset(message.OccurredAt).ToUnixTimeSeconds())
        };

        await channel.BasicPublishAsync(
            RabbitMqTopology.Exchange,
            RabbitMqTopology.RoutingKey,
            mandatory: true,
            properties,
            body,
            cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        if (_channel is not null)
        {
            await _channel.DisposeAsync();
        }
    }

    private async Task<IChannel> GetChannelAsync(CancellationToken cancellationToken)
    {
        if (_channel is { IsOpen: true })
        {
            return _channel;
        }

        if (_channel is not null)
        {
            await _channel.DisposeAsync();
        }

        var connection = await _connectionProvider.GetConnectionAsync(cancellationToken);
        _channel = await connection.CreateChannelAsync(ChannelOptions, cancellationToken);

        await _topology.DeclareAsync(_channel, cancellationToken);

        return _channel;
    }

    private static ServiceRequestEventMessage ToContract(OutboxMessageEntity message)
    {
        return new ServiceRequestEventMessage(
            message.ServiceRequestId,
            message.OldStatus,
            message.NewStatus,
            message.Priority,
            message.OccurredAt);
    }
}
