using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;
using RequestManagementService.Application.Behaviors;
using RequestManagementService.Application.Features.ServiceRequests.ChangeServiceRequestStatus;
using RequestManagementService.Application.Messaging;
using RequestManagementService.Application.Messaging.Options;
using RequestManagementService.Application.Messaging.Outbox;
using RequestManagementService.Application.Messaging.RabbitMq;
using RequestManagementService.DataAccess;
using RequestManagementService.DataAccess.Repositories;

namespace RequestManagementService.API.Extensions;

public static class ServiceCollectionExtensions
{
    private const string PostgresHost = "POSTGRES_HOST";
    private const string PostgresPort = "POSTGRES_PORT";
    private const string PostgresDb = "POSTGRES_DB";
    private const string PostgresUser = "POSTGRES_USER";
    private const string PostgresPassword = "POSTGRES_PASSWORD";

    private const string RabbitMqHost = "RABBITMQ_HOST";
    private const string RabbitMqPort = "RABBITMQ_PORT";
    private const string RabbitMqUser = "RABBITMQ_USER";
    private const string RabbitMqPassword = "RABBITMQ_PASSWORD";
    private const string RabbitMqVirtualHost = "RABBITMQ_VHOST";

    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(ValidationBehavior<,>).Assembly;

        services.AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssembly(assembly);
            configuration.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssembly(assembly, includeInternalTypes: true);

        services.AddSingleton<IServiceRequestStatusTransitions, ServiceRequestStatusTransitions>();

        return services;
    }

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

    public static IServiceCollection AddOutboxPublishing(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddRabbitMq(configuration);
        services.Configure<OutboxOptions>(configuration.GetSection(OutboxOptions.SectionName));

        services.AddSingleton<IOutboxMessagePublisher, RabbitMqOutboxMessagePublisher>();
        services.AddHostedService<OutboxPublisher>();

        return services;
    }

    public static IServiceCollection AddEventConsuming(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddRabbitMq(configuration);
        services.AddHostedService<ServiceRequestEventConsumer>();

        return services;
    }

    private static void AddRabbitMq(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RabbitMqOptions>(options =>
        {
            options.HostName = GetRequiredValue(configuration, RabbitMqHost);
            options.Port = int.Parse(GetRequiredValue(configuration, RabbitMqPort));
            options.UserName = GetRequiredValue(configuration, RabbitMqUser);
            options.Password = GetRequiredValue(configuration, RabbitMqPassword);
            options.VirtualHost = GetRequiredValue(configuration, RabbitMqVirtualHost);
        });

        services.AddSingleton<MessageSerializer>();
        services.AddSingleton<RabbitMqConnectionProvider>();
        services.AddSingleton<RabbitMqTopology>();
    }

    private static string GetRequiredValue(IConfiguration configuration, string key)
    {
        return configuration[key] ?? throw new InvalidOperationException($"Configuration value '{key}' is not set.");
    }
}
