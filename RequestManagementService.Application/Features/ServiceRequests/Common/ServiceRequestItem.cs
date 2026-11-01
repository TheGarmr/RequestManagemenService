using RequestManagementService.DataAccess.Enums;

namespace RequestManagementService.Application.Features.ServiceRequests.Common;

public sealed record ServiceRequestItem(
    Guid Id,
    string Title,
    string? Description,
    ServiceRequestPriority Priority,
    ServiceRequestStatus Status);
