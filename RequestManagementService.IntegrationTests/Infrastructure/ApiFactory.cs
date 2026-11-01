using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MysticMind.PostgresEmbed;
using RequestManagementService.API;
using RequestManagementService.DataAccess;
using RequestManagementService.DataAccess.Extensions;

namespace RequestManagementService.IntegrationTests.Infrastructure;

public sealed class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PgServer _postgres = new("18.6.0", clearInstanceDirOnStop: true);

    public async ValueTask InitializeAsync()
    {
        await _postgres.StartAsync();
        await Services.ApplyMigrationsAsync();
    }

    public async Task<T> QueryAsync<T>(Func<RequestManagementDbContext, Task<T>> query)
    {
        await using var scope = Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<RequestManagementDbContext>();

        return await query(dbContext);
    }

    public async Task ResetDatabaseAsync()
    {
        await using var scope = Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<RequestManagementDbContext>();

        await dbContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE service_requests, outbox_messages");
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        _postgres.Dispose();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("POSTGRES_HOST", "localhost");
        builder.UseSetting("POSTGRES_PORT", _postgres.PgPort.ToString());
        builder.UseSetting("POSTGRES_DB", "request_management_tests");
        builder.UseSetting("POSTGRES_USER", "postgres");
        builder.UseSetting("POSTGRES_PASSWORD", "postgres");

        builder.ConfigureTestServices(services =>
            services.Remove(services.Single(descriptor => descriptor.ImplementationType?.Name == "OutboxPublisher")));
    }
}
