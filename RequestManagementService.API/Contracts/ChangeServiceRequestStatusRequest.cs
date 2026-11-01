using System.Text.Json.Serialization;
using RequestManagementService.DataAccess.Enums;

namespace RequestManagementService.API.Contracts;

public sealed record ChangeServiceRequestStatusRequest([property: JsonRequired] ServiceRequestStatus Status);
