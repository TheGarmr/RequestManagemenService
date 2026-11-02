using Microsoft.Extensions.Diagnostics.HealthChecks;
using RequestManagementService.Application.Messaging.RabbitMq;

namespace RequestManagementService.API.Extensions;

public static class HealthChecksBuilderExtensions
{
    public static IHealthChecksBuilder AddRabbitMqCheck(this IHealthChecksBuilder builder)
    {
        return builder.AddCheck<RabbitMqHealthCheck>(
            "rabbitmq",
            HealthStatus.Degraded,
            timeout: TimeSpan.FromSeconds(5));
    }
}
