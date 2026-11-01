using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RequestManagementService.DataAccess.Repositories;

namespace RequestManagementService.DataAccess.Extensions;

public static class ServiceCollectionExtensions
{
    private const string RequestManagementConnectionStringName = "RequestManagement";
    
    public static IServiceCollection AddDataAccess(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(RequestManagementConnectionStringName)
            ?? throw new InvalidOperationException($"Connection string '{RequestManagementConnectionStringName}' is not configured.");

        services.TryAddSingleton(TimeProvider.System);

        services.AddDbContext<RequestManagementDbContext>(options => options
            .UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention());

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IServiceRequestRepository, ServiceRequestRepository>();
        services.AddScoped<IOutboxRepository, OutboxRepository>();
        services.AddScoped<IServiceRequestEventRepository, ServiceRequestEventRepository>();

        return services;
    }
}
