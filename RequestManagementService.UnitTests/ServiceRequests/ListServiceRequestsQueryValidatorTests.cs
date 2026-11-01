using RequestManagementService.Application.Features.ServiceRequests.ListServiceRequests;

namespace RequestManagementService.UnitTests.ServiceRequests;

public sealed class ListServiceRequestsQueryValidatorTests
{
    private readonly ListServiceRequestsQueryValidator _validator = new();

    [Theory]
    [InlineData(null)]
    [InlineData("New")]
    [InlineData("inprogress")]
    public void Validate_WithKnownOrMissingStatus_Passes(string? status)
    {
        var result = _validator.Validate(new ListServiceRequestsQuery(status, 1, 20));

        result.IsValid.ShouldBeTrue();
    }

    [Theory]
    [InlineData("Unknown")]
    [InlineData("1")]
    public void Validate_WithUnknownStatus_Fails(string status)
    {
        var result = _validator.Validate(new ListServiceRequestsQuery(status, 1, 20));

        result.Errors.ShouldContain(error => error.PropertyName == nameof(ListServiceRequestsQuery.Status));
    }

    [Theory]
    [InlineData(0, 20, nameof(ListServiceRequestsQuery.Page))]
    [InlineData(1, 0, nameof(ListServiceRequestsQuery.PageSize))]
    [InlineData(1, 101, nameof(ListServiceRequestsQuery.PageSize))]
    public void Validate_WithOutOfRangePaging_Fails(int page, int pageSize, string propertyName)
    {
        var result = _validator.Validate(new ListServiceRequestsQuery(null, page, pageSize));

        result.Errors.ShouldContain(error => error.PropertyName == propertyName);
    }
}
