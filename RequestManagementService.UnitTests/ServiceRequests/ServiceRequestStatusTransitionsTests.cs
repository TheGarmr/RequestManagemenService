using RequestManagementService.Application.Features.ServiceRequests.ChangeServiceRequestStatus;
using RequestManagementService.DataAccess.Enums;

namespace RequestManagementService.UnitTests.ServiceRequests;

public sealed class ServiceRequestStatusTransitionsTests
{
    private readonly ServiceRequestStatusTransitions _transitions = new();

    public static TheoryData<ServiceRequestStatus, ServiceRequestStatus, bool> AllTransitions => new()
    {
        { ServiceRequestStatus.New, ServiceRequestStatus.New, false },
        { ServiceRequestStatus.New, ServiceRequestStatus.InProgress, true },
        { ServiceRequestStatus.New, ServiceRequestStatus.Done, false },
        { ServiceRequestStatus.New, ServiceRequestStatus.Cancelled, true },
        { ServiceRequestStatus.InProgress, ServiceRequestStatus.New, false },
        { ServiceRequestStatus.InProgress, ServiceRequestStatus.InProgress, false },
        { ServiceRequestStatus.InProgress, ServiceRequestStatus.Done, true },
        { ServiceRequestStatus.InProgress, ServiceRequestStatus.Cancelled, true },
        { ServiceRequestStatus.Done, ServiceRequestStatus.New, false },
        { ServiceRequestStatus.Done, ServiceRequestStatus.InProgress, false },
        { ServiceRequestStatus.Done, ServiceRequestStatus.Done, false },
        { ServiceRequestStatus.Done, ServiceRequestStatus.Cancelled, false },
        { ServiceRequestStatus.Cancelled, ServiceRequestStatus.New, true },
        { ServiceRequestStatus.Cancelled, ServiceRequestStatus.InProgress, false },
        { ServiceRequestStatus.Cancelled, ServiceRequestStatus.Done, false },
        { ServiceRequestStatus.Cancelled, ServiceRequestStatus.Cancelled, false }
    };

    [Theory]
    [MemberData(nameof(AllTransitions))]
    public void IsAllowed_ReturnsExpectedResult(ServiceRequestStatus from, ServiceRequestStatus to, bool expected)
    {
        _transitions.IsAllowed(from, to).ShouldBe(expected);
    }
}
