using RequestManagementService.DataAccess.Enums;

namespace RequestManagementService.Application.Features.ServiceRequests.ChangeServiceRequestStatus;

public interface IServiceRequestStatusTransitions
{
    bool IsAllowed(ServiceRequestStatus from, ServiceRequestStatus to);
}
