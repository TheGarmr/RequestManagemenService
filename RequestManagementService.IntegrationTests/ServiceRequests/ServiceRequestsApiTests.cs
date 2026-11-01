using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RequestManagementService.Application.Common;
using RequestManagementService.Application.Features.ServiceRequests.Common;
using RequestManagementService.DataAccess;
using RequestManagementService.Domain.ServiceRequests;
using RequestManagementService.IntegrationTests.Infrastructure;

namespace RequestManagementService.IntegrationTests.ServiceRequests;

[Collection(ApiCollection.Name)]
public sealed class ServiceRequestsApiTests(ApiFactory factory)
{
    private const string BaseUrl = "/api/service-requests";

    private readonly HttpClient _client = factory.CreateClient();
    private readonly CancellationToken _cancellationToken = TestContext.Current.CancellationToken;

    [Fact]
    public async Task Create_ReturnsCreatedWithLocationAndDefaults()
    {
        var response = await _client.PostAsJsonAsync(BaseUrl, new { title = "  Replace pump bearing  " }, TestJson.Options, _cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Created);

        var item = await ReadAsync<ServiceRequestItem>(response);
        item.Title.ShouldBe("Replace pump bearing");
        item.Priority.ShouldBe(ServiceRequestPriority.Normal);
        item.Status.ShouldBe(ServiceRequestStatus.New);
        response.Headers.Location!.AbsolutePath.ShouldBe($"{BaseUrl}/{item.Id}");
    }

    [Fact]
    public async Task Create_SetsUtcTimestamps()
    {
        var item = await CreateAsync("Inspect conveyor belt");

        var entity = await FindEntityAsync(item.Id);
        entity.CreatedAt.Kind.ShouldBe(DateTimeKind.Utc);
        entity.UpdatedAt.ShouldBe(entity.CreatedAt);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Create_WithEmptyTitle_ReturnsValidationProblem(string title)
    {
        var response = await _client.PostAsJsonAsync(BaseUrl, new { title }, TestJson.Options, _cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var problem = await ReadAsync<ValidationProblemDetails>(response);
        problem.Errors.ShouldContainKey("title");
    }

    [Fact]
    public async Task Create_WithTooLongTitle_ReturnsValidationProblem()
    {
        var response = await _client.PostAsJsonAsync(BaseUrl, new { title = new string('a', 201) }, TestJson.Options, _cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var problem = await ReadAsync<ValidationProblemDetails>(response);
        problem.Errors.ShouldContainKey("title");
    }

    [Fact]
    public async Task Create_WithUnknownPriority_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync(BaseUrl, new { title = "Title", priority = "Urgent" }, TestJson.Options, _cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType!.MediaType.ShouldBe("application/problem+json");
    }

    [Fact]
    public async Task GetById_WhenExists_ReturnsServiceRequest()
    {
        var created = await CreateAsync("Calibrate sensor", ServiceRequestPriority.High);

        var response = await _client.GetAsync($"{BaseUrl}/{created.Id}", _cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var item = await ReadAsync<ServiceRequestItem>(response);
        item.ShouldBe(created);
    }

    [Theory]
    [InlineData("0192f0a0-0000-7000-8000-000000000000")]
    [InlineData("not-a-guid")]
    public async Task GetById_WhenMissing_ReturnsNotFoundProblem(string id)
    {
        var response = await _client.GetAsync($"{BaseUrl}/{id}", _cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        response.Content.Headers.ContentType!.MediaType.ShouldBe("application/problem+json");
    }

    [Fact]
    public async Task ChangeStatus_AlongNormalFlow_ReturnsUpdatedResource()
    {
        var created = await CreateAsync("Replace filter");

        var inProgress = await ChangeStatusAsync(created.Id, "InProgress");
        inProgress.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await ReadAsync<ServiceRequestItem>(inProgress)).Status.ShouldBe(ServiceRequestStatus.InProgress);

        var done = await ChangeStatusAsync(created.Id, "Done");
        done.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await ReadAsync<ServiceRequestItem>(done)).Status.ShouldBe(ServiceRequestStatus.Done);
    }

    [Fact]
    public async Task ChangeStatus_UpdatesUpdatedAtOnly()
    {
        var created = await CreateAsync("Tighten bolts");
        var before = await FindEntityAsync(created.Id);

        await ChangeStatusAsync(created.Id, "InProgress");

        var after = await FindEntityAsync(created.Id);
        after.CreatedAt.ShouldBe(before.CreatedAt);
        after.UpdatedAt.ShouldBeGreaterThan(before.UpdatedAt);
    }

    [Theory]
    [InlineData("New")]
    [InlineData("Done")]
    public async Task ChangeStatus_WhenTransitionIsForbidden_ReturnsConflict(string status)
    {
        var created = await CreateAsync("Check wiring");

        var response = await ChangeStatusAsync(created.Id, status);

        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        response.Content.Headers.ContentType!.MediaType.ShouldBe("application/problem+json");
    }

    [Fact]
    public async Task ChangeStatus_FromDone_ReturnsConflict()
    {
        var created = await CreateAsync("Lubricate chain");
        await ChangeStatusAsync(created.Id, "InProgress");
        await ChangeStatusAsync(created.Id, "Done");

        var response = await ChangeStatusAsync(created.Id, "New");

        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task ChangeStatus_CancelledCanBeReopened()
    {
        var created = await CreateAsync("Replace belt");
        await ChangeStatusAsync(created.Id, "Cancelled");

        var response = await ChangeStatusAsync(created.Id, "New");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ChangeStatus_WhenMissing_ReturnsNotFound()
    {
        var response = await ChangeStatusAsync(Guid.CreateVersion7(), "InProgress");

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ChangeStatus_WithoutStatus_ReturnsBadRequestAndKeepsStatus()
    {
        var created = await CreateAsync("Clean nozzle");

        var response = await _client.PatchAsJsonAsync($"{BaseUrl}/{created.Id}/status", new { }, TestJson.Options, _cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType!.MediaType.ShouldBe("application/problem+json");
        (await FindEntityAsync(created.Id)).Status.ShouldBe(ServiceRequestStatus.New);
    }

    [Fact]
    public async Task List_ReturnsPageSortedByNewestFirst()
    {
        await factory.ResetDatabaseAsync();
        var first = await CreateAsync("First");
        var second = await CreateAsync("Second");
        var third = await CreateAsync("Third");

        var response = await _client.GetAsync($"{BaseUrl}?page=1&pageSize=2", _cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var page = await ReadAsync<PagedResult<ServiceRequestItem>>(response);
        page.TotalCount.ShouldBe(3);
        page.Page.ShouldBe(1);
        page.PageSize.ShouldBe(2);
        page.Items.Select(item => item.Id).ShouldBe([third.Id, second.Id]);

        var secondPage = await ReadAsync<PagedResult<ServiceRequestItem>>(await _client.GetAsync($"{BaseUrl}?page=2&pageSize=2", _cancellationToken));
        secondPage.Items.Select(item => item.Id).ShouldBe([first.Id]);
    }

    [Fact]
    public async Task List_FiltersByStatus()
    {
        await factory.ResetDatabaseAsync();
        await CreateAsync("Stays new");
        var started = await CreateAsync("Gets started");
        await ChangeStatusAsync(started.Id, "InProgress");

        var page = await ReadAsync<PagedResult<ServiceRequestItem>>(await _client.GetAsync($"{BaseUrl}?status=InProgress", _cancellationToken));

        page.TotalCount.ShouldBe(1);
        page.Items.Single().Id.ShouldBe(started.Id);
    }

    [Theory]
    [InlineData("?page=0")]
    [InlineData("?pageSize=101")]
    [InlineData("?status=Unknown")]
    [InlineData("?status=1")]
    public async Task List_WithInvalidQuery_ReturnsValidationProblem(string query)
    {
        var response = await _client.GetAsync($"{BaseUrl}{query}", _cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType!.MediaType.ShouldBe("application/problem+json");
    }

    private async Task<ServiceRequestItem> CreateAsync(string title, ServiceRequestPriority priority = ServiceRequestPriority.Normal)
    {
        var response = await _client.PostAsJsonAsync(BaseUrl, new { title, priority }, TestJson.Options, _cancellationToken);
        response.StatusCode.ShouldBe(HttpStatusCode.Created);

        return await ReadAsync<ServiceRequestItem>(response);
    }

    private Task<HttpResponseMessage> ChangeStatusAsync(Guid id, string status)
    {
        return _client.PatchAsJsonAsync($"{BaseUrl}/{id}/status", new { status }, TestJson.Options, _cancellationToken);
    }

    private async Task<DataAccess.Entities.ServiceRequestEntity> FindEntityAsync(Guid id)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<RequestManagementDbContext>();

        return await dbContext.ServiceRequests.AsNoTracking().SingleAsync(x => x.Id == id, _cancellationToken);
    }

    private async Task<T> ReadAsync<T>(HttpResponseMessage response)
        where T : class
    {
        var result = await response.Content.ReadFromJsonAsync<T>(TestJson.Options, _cancellationToken);
        result.ShouldNotBeNull();

        return result;
    }
}
