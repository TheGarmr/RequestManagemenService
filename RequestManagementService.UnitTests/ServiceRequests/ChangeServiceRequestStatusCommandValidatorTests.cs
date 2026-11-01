using RequestManagementService.Application.Features.ServiceRequests.ChangeServiceRequestStatus;
using RequestManagementService.Domain.ServiceRequests;

namespace RequestManagementService.UnitTests.ServiceRequests;

public sealed class ChangeServiceRequestStatusCommandValidatorTests
{
    private readonly ChangeServiceRequestStatusCommandValidator _validator = new();

    [Fact]
    public void Validate_WithStatus_Passes()
    {
        var result = _validator.Validate(new ChangeServiceRequestStatusCommand(Guid.CreateVersion7(), ServiceRequestStatus.InProgress));

        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Validate_WithUndefinedStatus_Fails()
    {
        var result = _validator.Validate(new ChangeServiceRequestStatusCommand(Guid.CreateVersion7(), (ServiceRequestStatus)42));

        result.Errors.ShouldContain(error => error.PropertyName == nameof(ChangeServiceRequestStatusCommand.Status));
    }
}
