using System.Text.Json;
using System.Text.Json.Serialization;
using RabbitMQ.Client;
using RequestManagementService.Application.Messaging.Contracts;
using RequestManagementService.Application.Messaging.Outbox;
using RequestManagementService.DataAccess.Entities;

namespace RequestManagementService.Application.Messaging.RabbitMq;

internal sealed class RabbitMqOutboxMessagePublisher : IOutboxMessagePublisher, IAsyncDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private static readonly CreateChannelOptions ChannelOptions = new(
        publisherConfirmationsEnabled: true,
        publisherConfirmationTrackingEnabled: true);

    private readonly RabbitMqConnectionProvider _connectionProvider;
    private readonly RabbitMqTopology _topology;
    private IChannel? _channel;

    public RabbitMqOutboxMessagePublisher(RabbitMqConnectionProvider connectionProvider, RabbitMqTopology topology)
    {
        _connectionProvider = connectionProvider;
        _topology = topology;
    }

    public async Task PublishAsync(OutboxMessageEntity message, CancellationToken cancellationToken)
    {
        var channel = await GetChannelAsync(cancellationToken);

        var body = JsonSerializer.SerializeToUtf8Bytes(ToContract(message), JsonOptions);
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
