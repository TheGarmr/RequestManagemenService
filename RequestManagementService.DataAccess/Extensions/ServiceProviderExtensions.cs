using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace RequestManagementService.DataAccess.Extensions;

public static class ServiceProviderExtensions
{
    public static async Task ApplyMigrationsAsync(this IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<RequestManagementDbContext>();

        await dbContext.Database.MigrateAsync();
    }
}
