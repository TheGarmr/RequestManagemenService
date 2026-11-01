using RequestManagementService.Domain.ServiceRequests;

namespace RequestManagementService.API.Contracts;

public sealed record CreateServiceRequestRequest(
    string Title,
    string? Description,
    ServiceRequestPriority Priority = ServiceRequestPriority.Normal);
