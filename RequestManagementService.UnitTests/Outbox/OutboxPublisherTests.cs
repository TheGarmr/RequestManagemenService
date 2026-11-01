using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using RequestManagementService.Application.Messaging.Options;
using RequestManagementService.Application.Messaging.Outbox;
using RequestManagementService.DataAccess;
using RequestManagementService.DataAccess.Entities;
using RequestManagementService.DataAccess.Enums;
using RequestManagementService.DataAccess.Repositories;

namespace RequestManagementService.UnitTests.Outbox;

public sealed class OutboxPublisherTests
{
    private readonly IOutboxRepository _outboxRepository = Substitute.For<IOutboxRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IOutboxMessagePublisher _messagePublisher = Substitute.For<IOutboxMessagePublisher>();
    private readonly OutboxPublisher _publisher;

    public OutboxPublisherTests()
    {
        var serviceProvider = Substitute.For<IServiceProvider>();
        serviceProvider.GetService(typeof(IOutboxRepository)).Returns(_outboxRepository);
        serviceProvider.GetService(typeof(IUnitOfWork)).Returns(_unitOfWork);

        var scope = Substitute.For<IServiceScope>();
        scope.ServiceProvider.Returns(serviceProvider);

        var scopeFactory = Substitute.For<IServiceScopeFactory>();
        scopeFactory.CreateScope().Returns(scope);

        var options = Options.Create(new OutboxOptions
        {
            BatchSize = 10,
            PollingInterval = TimeSpan.FromSeconds(1),
            MaxRetryDelay = TimeSpan.FromSeconds(30)
        });

        _publisher = new OutboxPublisher(
            scopeFactory,
            _messagePublisher,
            options,
            TimeProvider.System,
            NullLogger<OutboxPublisher>.Instance);
    }

    [Fact]
    public async Task PublishBatchAsync_WhenAllMessagesArePublished_MarksThemAsProcessed()
    {
        var messages = new List<OutboxMessageEntity> { CreateMessage(), CreateMessage() };
        _outboxRepository.GetUnprocessedAsync(10, Arg.Any<CancellationToken>()).Returns(messages);

        var published = await _publisher.PublishBatchAsync(CancellationToken.None);

        published.ShouldBe(2);
        messages.ShouldAllBe(message => message.ProcessedAt != null);
        await _messagePublisher.Received(2).PublishAsync(Arg.Any<OutboxMessageEntity>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(2).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PublishBatchAsync_WhenPublishFails_KeepsPublishedMessagesAndRethrows()
    {
        var first = CreateMessage();
        var second = CreateMessage();
        var third = CreateMessage();
        _outboxRepository.GetUnprocessedAsync(10, Arg.Any<CancellationToken>()).Returns([first, second, third]);
        _messagePublisher.PublishAsync(second, Arg.Any<CancellationToken>()).ThrowsAsync(new InvalidOperationException("Broker is unavailable."));

        await Should.ThrowAsync<InvalidOperationException>(() => _publisher.PublishBatchAsync(CancellationToken.None));

        first.ProcessedAt.ShouldNotBeNull();
        second.ProcessedAt.ShouldBeNull();
        third.ProcessedAt.ShouldBeNull();
        await _messagePublisher.DidNotReceive().PublishAsync(third, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PublishBatchAsync_WhenOutboxIsEmpty_PublishesNothing()
    {
        _outboxRepository.GetUnprocessedAsync(10, Arg.Any<CancellationToken>()).Returns([]);

        var published = await _publisher.PublishBatchAsync(CancellationToken.None);

        published.ShouldBe(0);
        await _messagePublisher.DidNotReceive().PublishAsync(Arg.Any<OutboxMessageEntity>(), Arg.Any<CancellationToken>());
    }

    private static OutboxMessageEntity CreateMessage()
    {
        return new OutboxMessageEntity
        {
            ServiceRequestId = Guid.CreateVersion7(),
            NewStatus = ServiceRequestStatus.New,
            Priority = ServiceRequestPriority.Normal
        };
    }
}
