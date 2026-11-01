namespace RequestManagementService.Domain.ServiceRequests;

public sealed class ServiceRequest
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public ServiceRequestPriority Priority { get; set; } = ServiceRequestPriority.Normal;

    public ServiceRequestStatus Status { get; set; } = ServiceRequestStatus.New;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
