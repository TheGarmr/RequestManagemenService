using RequestManagementService.DataAccess.Enums;

namespace RequestManagementService.Application.Messaging.Contracts;

public sealed record ServiceRequestEventMessage(
    Guid ServiceRequestId,
    ServiceRequestStatus? OldStatus,
    ServiceRequestStatus NewStatus,
    ServiceRequestPriority Priority,
    DateTime OccurredAt);
