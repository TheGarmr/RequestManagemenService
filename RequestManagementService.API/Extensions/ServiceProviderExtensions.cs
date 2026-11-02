using Microsoft.EntityFrameworkCore;
using RequestManagementService.DataAccess;

namespace RequestManagementService.API.Extensions;

public static class ServiceProviderExtensions
{
    public static async Task ApplyMigrationsAsync(this IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<RequestManagementDbContext>();

        await dbContext.Database.MigrateAsync();
    }
}
