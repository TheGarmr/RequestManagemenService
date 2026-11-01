using RequestManagementService.DataAccess.Enums;

namespace RequestManagementService.DataAccess.Entities;

public sealed class ServiceRequestEntity
{
    public Guid Id { get; set; }

    public required string Title { get; set; }

    public string? Description { get; set; }

    public ServiceRequestPriority Priority { get; set; }

    public ServiceRequestStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public uint Version { get; set; }
}
