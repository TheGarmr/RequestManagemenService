using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RequestManagementService.Application.Mappers;
using RequestManagementService.Application.Messaging.Contracts;

namespace RequestManagementService.Application.Messaging.RabbitMq;

public sealed class ServiceRequestEventConsumer : BackgroundService
{
    private const ushort PrefetchCount = 10;

    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(5);

    private readonly RabbitMqConnectionProvider _connectionProvider;
    private readonly RabbitMqTopology _topology;
    private readonly MessageSerializer _serializer;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<ServiceRequestEventConsumer> _logger;
    private IChannel? _channel;

    public ServiceRequestEventConsumer(
        RabbitMqConnectionProvider connectionProvider,
        RabbitMqTopology topology,
        MessageSerializer serializer,
        IServiceScopeFactory scopeFactory,
        TimeProvider timeProvider,
        ILogger<ServiceRequestEventConsumer> logger)
    {
        _connectionProvider = connectionProvider;
        _topology = topology;
        _serializer = serializer;
        _scopeFactory = scopeFactory;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public override void Dispose()
    {
        _channel?.Dispose();
        base.Dispose();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await StartConsumingAsync(stoppingToken);
                return;
            }
            catch (Exception exception) when (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogError(exception, "Failed to start consuming {Queue}, next try in {Delay}", RabbitMqTopology.Queue, RetryDelay);

                await Task.Delay(RetryDelay, _timeProvider, stoppingToken);
            }
        }
    }

    private async Task StartConsumingAsync(CancellationToken cancellationToken)
    {
        _channel?.Dispose();

        var connection = await _connectionProvider.GetConnectionAsync(cancellationToken);
        _channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await _topology.DeclareAsync(_channel, cancellationToken);
        await _channel.BasicQosAsync(0, PrefetchCount, false, cancellationToken);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += HandleMessageAsync;

        await _channel.BasicConsumeAsync(RabbitMqTopology.Queue, autoAck: false, consumer, cancellationToken);
    }

    private async Task HandleMessageAsync(object source, BasicDeliverEventArgs args)
    {
        if (!long.TryParse(args.BasicProperties.MessageId, out var messageId)
            || !_serializer.TryDeserialize<ServiceRequestEventMessage>(args.Body, out var serviceRequest))
        {
            _logger.LogError("Message {MessageId} is malformed, sending it to the dead letter queue", args.BasicProperties.MessageId);
            await _channel!.BasicRejectAsync(args.DeliveryTag, requeue: false, args.CancellationToken);
            return;
        }

        _logger.LogInformation(
            "Received message about service request with id {ServiceRequestId}",
            serviceRequest.ServiceRequestId);

        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();

            await sender.Send(ServiceRequestEventMapper.ToCommand(messageId, serviceRequest), args.CancellationToken);
            await _channel!.BasicAckAsync(args.DeliveryTag, multiple: false, args.CancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to process message {MessageId}, returning it to the queue", messageId);
            await _channel!.BasicNackAsync(args.DeliveryTag, multiple: false, requeue: true, args.CancellationToken);
        }
    }
}
