using RequestManagementService.DataAccess.Enums;

namespace RequestManagementService.API.Contracts;

public sealed record CreateServiceRequestRequest(
    string Title,
    string? Description,
    ServiceRequestPriority Priority = ServiceRequestPriority.Normal);
