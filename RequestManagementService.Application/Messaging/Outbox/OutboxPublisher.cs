using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RequestManagementService.Application.Messaging.Options;
using RequestManagementService.DataAccess;
using RequestManagementService.DataAccess.Repositories;

namespace RequestManagementService.Application.Messaging.Outbox;

public sealed class OutboxPublisher : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IOutboxMessagePublisher _messagePublisher;
    private readonly IOptions<OutboxOptions> _options;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<OutboxPublisher> _logger;

    public OutboxPublisher(
        IServiceScopeFactory scopeFactory,
        IOutboxMessagePublisher messagePublisher,
        IOptions<OutboxOptions> options,
        TimeProvider timeProvider,
        ILogger<OutboxPublisher> logger)
    {
        _scopeFactory = scopeFactory;
        _messagePublisher = messagePublisher;
        _options = options;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var failedAttempts = 0;

        while (!stoppingToken.IsCancellationRequested)
        {
            TimeSpan delay;

            try
            {
                var published = await PublishBatchAsync(stoppingToken);

                failedAttempts = 0;
                delay = published == _options.Value.BatchSize ? TimeSpan.Zero : _options.Value.PollingInterval;
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                failedAttempts++;
                delay = GetRetryDelay(failedAttempts);

                _logger.LogError(exception, "Outbox publishing failed, attempt {Attempt}, next try in {Delay}", failedAttempts, delay);
            }

            await Task.Delay(delay, _timeProvider, stoppingToken);
        }
    }

    internal async Task<int> PublishBatchAsync(CancellationToken cancellationToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var outboxRepository = scope.ServiceProvider.GetRequiredService<IOutboxRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var messages = await outboxRepository.GetUnprocessedAsync(_options.Value.BatchSize, cancellationToken);

        foreach (var message in messages)
        {
            await _messagePublisher.PublishAsync(message, cancellationToken);

            message.ProcessedAt = _timeProvider.GetUtcNow().UtcDateTime;
            await unitOfWork.CommitAsync(cancellationToken);
        }

        return messages.Count;
    }

    private TimeSpan GetRetryDelay(int failedAttempts)
    {
        var delay = _options.Value.PollingInterval * Math.Pow(2, Math.Min(failedAttempts, 10));

        return delay < _options.Value.MaxRetryDelay ? delay : _options.Value.MaxRetryDelay;
    }
}
