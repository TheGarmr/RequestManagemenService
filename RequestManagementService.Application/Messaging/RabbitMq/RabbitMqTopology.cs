using RabbitMQ.Client;

namespace RequestManagementService.Application.Messaging.RabbitMq;

public sealed class RabbitMqTopology
{
    public const string Exchange = "request-management.events";
    public const string DeadLetterExchange = "request-management.events.dlx";
    public const string RoutingKey = "service-management.service-request.upsert";
    public const string Queue = "service-management.service-request.upsert";
    public const string DeadLetterQueue = "service-management.service-request.upsert.dlq";

    private const int DeliveryLimit = 5;

    public async Task DeclareAsync(IChannel channel, CancellationToken cancellationToken)
    {
        await channel.ExchangeDeclareAsync(Exchange, ExchangeType.Topic, durable: true, autoDelete: false, cancellationToken: cancellationToken);
        await channel.ExchangeDeclareAsync(DeadLetterExchange, ExchangeType.Fanout, durable: true, autoDelete: false, cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(
            DeadLetterQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: new Dictionary<string, object?>
            {
                ["x-queue-type"] = "quorum"
            },
            cancellationToken: cancellationToken);

        await channel.QueueBindAsync(DeadLetterQueue, DeadLetterExchange, string.Empty, cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(
            Queue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: new Dictionary<string, object?>
            {
                ["x-queue-type"] = "quorum",
                ["x-dead-letter-exchange"] = DeadLetterExchange,
                ["x-dead-letter-strategy"] = "at-least-once",
                ["x-overflow"] = "reject-publish",
                ["x-delivery-limit"] = DeliveryLimit
            },
            cancellationToken: cancellationToken);

        await channel.QueueBindAsync(Queue, Exchange, RoutingKey, cancellationToken: cancellationToken);
    }
}
