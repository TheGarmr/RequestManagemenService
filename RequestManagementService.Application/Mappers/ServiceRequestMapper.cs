using RequestManagementService.Application.Features.ServiceRequests.Common;
using RequestManagementService.Application.Features.ServiceRequests.CreateServiceRequest;
using RequestManagementService.DataAccess.Entities;
using RequestManagementService.Domain.ServiceRequests;

namespace RequestManagementService.Application.Mappers;

internal static class ServiceRequestMapper
{
    public static ServiceRequestEntity ToEntity(CreateServiceRequestCommand command)
    {
        return new ServiceRequestEntity
        {
            Id = Guid.CreateVersion7(),
            Title = command.Title.Trim(),
            Description = command.Description,
            Priority = command.Priority,
            Status = ServiceRequestStatus.New
        };
    }

    public static ServiceRequestItem ToItem(ServiceRequestEntity entity)
    {
        return new ServiceRequestItem(
            entity.Id,
            entity.Title,
            entity.Description,
            entity.Priority,
            entity.Status);
    }
}
