using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using RequestManagementService.Application.Behaviors;
using RequestManagementService.Application.Features.ServiceRequests.ChangeServiceRequestStatus;

namespace RequestManagementService.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(ServiceCollectionExtensions).Assembly;

        services.AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssembly(assembly);
            configuration.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssembly(assembly, includeInternalTypes: true);

        services.AddSingleton<IServiceRequestStatusTransitions, ServiceRequestStatusTransitions>();

        return services;
    }
}
