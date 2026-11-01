using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;
using RequestManagementService.DataAccess.Repositories;

namespace RequestManagementService.DataAccess.Extensions;

public static class ServiceCollectionExtensions
{
    private const string PostgresHost = "POSTGRES_HOST";
    private const string PostgresPort = "POSTGRES_PORT";
    private const string PostgresDb = "POSTGRES_DB";
    private const string PostgresUser = "POSTGRES_USER";
    private const string PostgresPassword = "POSTGRES_PASSWORD";

    public static IServiceCollection AddDataAccess(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = new NpgsqlConnectionStringBuilder
        {
            Host = GetRequiredValue(configuration, PostgresHost),
            Port = int.Parse(GetRequiredValue(configuration, PostgresPort)),
            Database = GetRequiredValue(configuration, PostgresDb),
            Username = GetRequiredValue(configuration, PostgresUser),
            Password = GetRequiredValue(configuration, PostgresPassword)
        }.ConnectionString;

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

    private static string GetRequiredValue(IConfiguration configuration, string key)
    {
        return configuration[key] ?? throw new InvalidOperationException($"Configuration value '{key}' is not set.");
    }
}
