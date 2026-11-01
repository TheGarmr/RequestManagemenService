using RequestManagementService.DataAccess.Entities;
using RequestManagementService.Domain.ServiceRequests;

namespace RequestManagementService.Application.Mappers;

internal static class ServiceRequestMapper
{
    public static ServiceRequest ToDomain(ServiceRequestEntity entity)
    {
        return new ServiceRequest
        {
            Id = entity.Id,
            Title = entity.Title,
            Description = entity.Description,
            Priority = entity.Priority,
            Status = entity.Status,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }

    public static ServiceRequestEntity ToEntity(ServiceRequest serviceRequest)
    {
        return new ServiceRequestEntity
        {
            Id = serviceRequest.Id,
            Title = serviceRequest.Title,
            Description = serviceRequest.Description,
            Priority = serviceRequest.Priority,
            Status = serviceRequest.Status
        };
    }

    public static void MapTo(ServiceRequest serviceRequest, ServiceRequestEntity entity)
    {
        entity.Title = serviceRequest.Title;
        entity.Description = serviceRequest.Description;
        entity.Priority = serviceRequest.Priority;
        entity.Status = serviceRequest.Status;
    }
}
