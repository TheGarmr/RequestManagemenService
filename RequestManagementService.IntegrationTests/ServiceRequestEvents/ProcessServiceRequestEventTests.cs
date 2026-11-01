using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RequestManagementService.Application.Features.ServiceRequestEvents.ProcessServiceRequestEvent;
using RequestManagementService.DataAccess.Enums;
using RequestManagementService.IntegrationTests.Infrastructure;

namespace RequestManagementService.IntegrationTests.ServiceRequestEvents;

[Collection(ApiCollection.Name)]
public sealed class ProcessServiceRequestEventTests
{
    private readonly ApiFactory _factory;
    private readonly CancellationToken _cancellationToken = TestContext.Current.CancellationToken;

    public ProcessServiceRequestEventTests(ApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Process_SavesEventOnceEvenWhenDeliveredTwice()
    {
        var command = new ProcessServiceRequestEventCommand(
            Random.Shared.NextInt64(1, long.MaxValue),
            Guid.CreateVersion7(),
            ServiceRequestStatus.InProgress,
            ServiceRequestStatus.Done,
            ServiceRequestPriority.Normal,
            new DateTime(2026, 11, 1, 10, 0, 0, DateTimeKind.Utc));

        await SendAsync(command);
        await SendAsync(command);

        var events = await _factory.QueryAsync(db => db.ServiceRequestEvents
            .AsNoTracking()
            .Where(x => x.MessageId == command.MessageId)
            .ToListAsync(_cancellationToken));

        var saved = events.ShouldHaveSingleItem();
        saved.ServiceRequestId.ShouldBe(command.ServiceRequestId);
        saved.OldStatus.ShouldBe(ServiceRequestStatus.InProgress);
        saved.NewStatus.ShouldBe(ServiceRequestStatus.Done);
        saved.Priority.ShouldBe(ServiceRequestPriority.Normal);
        saved.OccurredAt.ShouldBe(command.OccurredAt);
    }

    private async Task SendAsync(ProcessServiceRequestEventCommand command)
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        await sender.Send(command, _cancellationToken);
    }
}
