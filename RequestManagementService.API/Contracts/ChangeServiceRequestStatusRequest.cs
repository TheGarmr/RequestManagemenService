using System.Text.Json.Serialization;
using RequestManagementService.Domain.ServiceRequests;

namespace RequestManagementService.API.Contracts;

public sealed record ChangeServiceRequestStatusRequest([property: JsonRequired] ServiceRequestStatus Status);
