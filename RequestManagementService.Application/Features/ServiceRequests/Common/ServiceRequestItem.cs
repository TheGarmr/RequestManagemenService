using RequestManagementService.Domain.ServiceRequests;

namespace RequestManagementService.Application.Features.ServiceRequests.Common;

public sealed record ServiceRequestItem(
    Guid Id,
    string Title,
    string? Description,
    ServiceRequestPriority Priority,
    ServiceRequestStatus Status);
