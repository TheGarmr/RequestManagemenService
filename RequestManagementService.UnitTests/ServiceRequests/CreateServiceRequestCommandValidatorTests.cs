using RequestManagementService.Application.Features.ServiceRequests.CreateServiceRequest;
using RequestManagementService.DataAccess.Enums;

namespace RequestManagementService.UnitTests.ServiceRequests;

public sealed class CreateServiceRequestCommandValidatorTests
{
    private readonly CreateServiceRequestCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_Passes()
    {
        var result = _validator.Validate(new CreateServiceRequestCommand("Replace pump bearing", "Pump #3 is noisy", ServiceRequestPriority.High));

        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Validate_WithoutDescription_Passes()
    {
        var result = _validator.Validate(new CreateServiceRequestCommand("Replace pump bearing", null, ServiceRequestPriority.Normal));

        result.IsValid.ShouldBeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithEmptyTitle_Fails(string? title)
    {
        var result = _validator.Validate(new CreateServiceRequestCommand(title!, null, ServiceRequestPriority.Normal));

        result.Errors.ShouldContain(error => error.PropertyName == nameof(CreateServiceRequestCommand.Title));
    }

    [Theory]
    [InlineData(200, true)]
    [InlineData(201, false)]
    public void Validate_TitleLength_RespectsLimit(int length, bool expected)
    {
        var result = _validator.Validate(new CreateServiceRequestCommand(new string('a', length), null, ServiceRequestPriority.Normal));

        result.IsValid.ShouldBe(expected);
    }

    [Theory]
    [InlineData(2000, true)]
    [InlineData(2001, false)]
    public void Validate_DescriptionLength_RespectsLimit(int length, bool expected)
    {
        var result = _validator.Validate(new CreateServiceRequestCommand("Title", new string('a', length), ServiceRequestPriority.Normal));

        result.IsValid.ShouldBe(expected);
    }

    [Fact]
    public void Validate_WithUndefinedPriority_Fails()
    {
        var result = _validator.Validate(new CreateServiceRequestCommand("Title", null, (ServiceRequestPriority)42));

        result.Errors.ShouldContain(error => error.PropertyName == nameof(CreateServiceRequestCommand.Priority));
    }
}
