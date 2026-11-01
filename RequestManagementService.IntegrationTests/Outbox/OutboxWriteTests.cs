using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using RequestManagementService.Application.Features.ServiceRequests.Common;
using RequestManagementService.DataAccess.Enums;
using RequestManagementService.IntegrationTests.Infrastructure;

namespace RequestManagementService.IntegrationTests.Outbox;

[Collection(ApiCollection.Name)]
public sealed class OutboxWriteTests
{
    private const string BaseUrl = "/api/service-requests";

    private readonly ApiFactory _factory;
    private readonly HttpClient _client;
    private readonly CancellationToken _cancellationToken = TestContext.Current.CancellationToken;

    public OutboxWriteTests(ApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Create_WritesCreatedEventInSameTransaction()
    {
        var created = await CreateAsync("Replace pump bearing", ServiceRequestPriority.High);

        var message = await _factory.QueryAsync(db => db.OutboxMessages.AsNoTracking().SingleAsync(x => x.ServiceRequestId == created.Id, _cancellationToken));
        var entity = await _factory.QueryAsync(db => db.ServiceRequests.AsNoTracking().SingleAsync(x => x.Id == created.Id, _cancellationToken));

        message.OldStatus.ShouldBeNull();
        message.NewStatus.ShouldBe(ServiceRequestStatus.New);
        message.Priority.ShouldBe(ServiceRequestPriority.High);
        message.OccurredAt.ShouldBe(entity.UpdatedAt);
        message.ProcessedAt.ShouldBeNull();
    }

    [Fact]
    public async Task ChangeStatus_WritesStatusChangedEventInSameTransaction()
    {
        var created = await CreateAsync("Inspect conveyor belt", ServiceRequestPriority.Normal);

        var response = await _client.PatchAsJsonAsync($"{BaseUrl}/{created.Id}/status", new { status = "InProgress" }, TestJson.Options, _cancellationToken);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var messages = await _factory.QueryAsync(db => db.OutboxMessages.AsNoTracking()
            .Where(x => x.ServiceRequestId == created.Id)
            .OrderBy(x => x.Id)
            .ToListAsync(_cancellationToken));
        var entity = await _factory.QueryAsync(db => db.ServiceRequests.AsNoTracking().SingleAsync(x => x.Id == created.Id, _cancellationToken));

        messages.Count.ShouldBe(2);
        messages[1].OldStatus.ShouldBe(ServiceRequestStatus.New);
        messages[1].NewStatus.ShouldBe(ServiceRequestStatus.InProgress);
        messages[1].OccurredAt.ShouldBe(entity.UpdatedAt);
    }

    [Fact]
    public async Task ForbiddenTransition_DoesNotWriteEvent()
    {
        var created = await CreateAsync("Check wiring", ServiceRequestPriority.Low);

        var response = await _client.PatchAsJsonAsync($"{BaseUrl}/{created.Id}/status", new { status = "Done" }, TestJson.Options, _cancellationToken);
        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);

        var count = await _factory.QueryAsync(db => db.OutboxMessages.CountAsync(x => x.ServiceRequestId == created.Id, _cancellationToken));
        count.ShouldBe(1);
    }

    private async Task<ServiceRequestItem> CreateAsync(string title, ServiceRequestPriority priority)
    {
        var response = await _client.PostAsJsonAsync(BaseUrl, new { title, priority }, TestJson.Options, _cancellationToken);
        response.StatusCode.ShouldBe(HttpStatusCode.Created);

        var item = await response.Content.ReadFromJsonAsync<ServiceRequestItem>(TestJson.Options, _cancellationToken);
        item.ShouldNotBeNull();

        return item;
    }
}
