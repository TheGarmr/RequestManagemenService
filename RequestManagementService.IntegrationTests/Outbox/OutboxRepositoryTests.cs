using Microsoft.Extensions.DependencyInjection;
using RequestManagementService.DataAccess.Entities;
using RequestManagementService.DataAccess.Enums;
using RequestManagementService.DataAccess.Repositories;
using RequestManagementService.IntegrationTests.Infrastructure;

namespace RequestManagementService.IntegrationTests.Outbox;

[Collection(ApiCollection.Name)]
public sealed class OutboxRepositoryTests
{
    private readonly ApiFactory _factory;
    private readonly CancellationToken _cancellationToken = TestContext.Current.CancellationToken;

    public OutboxRepositoryTests(ApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetUnprocessedAsync_ReturnsOnlyUnprocessedMessagesInInsertionOrder()
    {
        await _factory.ResetDatabaseAsync();

        var processed = CreateMessage(DateTime.UtcNow);
        var first = CreateMessage(null);
        var second = CreateMessage(null);

        await _factory.QueryAsync(db =>
        {
            db.OutboxMessages.AddRange(processed, first, second);
            return db.SaveChangesAsync(_cancellationToken);
        });

        await using var scope = _factory.Services.CreateAsyncScope();
        var outboxRepository = scope.ServiceProvider.GetRequiredService<IOutboxRepository>();

        var messages = await outboxRepository.GetUnprocessedAsync(10, _cancellationToken);

        messages.Select(message => message.Id).ShouldBe([first.Id, second.Id]);
    }

    private static OutboxMessageEntity CreateMessage(DateTime? processedAt)
    {
        return new OutboxMessageEntity
        {
            ServiceRequestId = Guid.CreateVersion7(),
            NewStatus = ServiceRequestStatus.New,
            Priority = ServiceRequestPriority.Normal,
            ProcessedAt = processedAt
        };
    }
}
