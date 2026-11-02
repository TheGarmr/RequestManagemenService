using RequestManagementService.Application.Features.ServiceRequests.Common;
using RequestManagementService.Application.Features.ServiceRequests.CreateServiceRequest;
using RequestManagementService.DataAccess.Entities;
using RequestManagementService.DataAccess.Enums;

namespace RequestManagementService.Application.Mappers;

internal static class ServiceRequestMapper
{
    public static ServiceRequestEntity ToEntity(CreateServiceRequestCommand command, ServiceRequestStatus status)
    {
        return new ServiceRequestEntity
        {
            Id = Guid.CreateVersion7(),
            Title = command.Title.Trim(),
            Description = command.Description?.Trim(),
            Priority = command.Priority,
            Status = status
        };
    }

    public static OutboxMessageEntity ToOutboxMessage(ServiceRequestEntity entity, ServiceRequestStatus? oldStatus)
    {
        return new OutboxMessageEntity
        {
            ServiceRequestId = entity.Id,
            OldStatus = oldStatus,
            NewStatus = entity.Status,
            Priority = entity.Priority
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
