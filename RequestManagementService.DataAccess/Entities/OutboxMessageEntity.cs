using RequestManagementService.DataAccess.Enums;

namespace RequestManagementService.DataAccess.Entities;

public sealed class OutboxMessageEntity
{
    public long Id { get; set; }

    public Guid ServiceRequestId { get; set; }

    public ServiceRequestStatus? OldStatus { get; set; }

    public ServiceRequestStatus NewStatus { get; set; }

    public ServiceRequestPriority Priority { get; set; }

    public DateTime OccurredAt { get; set; }

    public DateTime? ProcessedAt { get; set; }
}
