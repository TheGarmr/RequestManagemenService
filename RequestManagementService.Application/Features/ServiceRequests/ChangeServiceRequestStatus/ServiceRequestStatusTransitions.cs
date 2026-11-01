using RequestManagementService.DataAccess.Enums;

namespace RequestManagementService.Application.Features.ServiceRequests.ChangeServiceRequestStatus;

internal sealed class ServiceRequestStatusTransitions : IServiceRequestStatusTransitions
{
    private static readonly Dictionary<ServiceRequestStatus, ServiceRequestStatus[]> AllowedTransitions = new()
    {
        [ServiceRequestStatus.New] = [ServiceRequestStatus.InProgress, ServiceRequestStatus.Cancelled],
        [ServiceRequestStatus.InProgress] = [ServiceRequestStatus.Done, ServiceRequestStatus.Cancelled],
        [ServiceRequestStatus.Cancelled] = [ServiceRequestStatus.New],
        [ServiceRequestStatus.Done] = []
    };

    public bool IsAllowed(ServiceRequestStatus from, ServiceRequestStatus to)
    {
        return AllowedTransitions[from].Contains(to);
    }
}
