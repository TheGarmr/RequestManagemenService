using RequestManagementService.Domain.ServiceRequests;

namespace RequestManagementService.API.Contracts;

public sealed record ChangeServiceRequestStatusRequest(ServiceRequestStatus? Status);
