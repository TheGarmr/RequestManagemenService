using RequestManagementService.DataAccess.Enums;

namespace RequestManagementService.Application.Features.ServiceRequests.ChangeServiceRequestStatus;

internal interface IServiceRequestStatusTransitions
{
    bool IsAllowed(ServiceRequestStatus from, ServiceRequestStatus to);
}
