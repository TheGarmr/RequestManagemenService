using FluentValidation;
using RequestManagementService.DataAccess.Enums;

namespace RequestManagementService.Application.Features.ServiceRequests.ListServiceRequests;

internal sealed class ListServiceRequestsQueryValidator : AbstractValidator<ListServiceRequestsQuery>
{
    private static readonly string[] StatusNames = Enum.GetNames<ServiceRequestStatus>();

    public ListServiceRequestsQueryValidator()
    {
        RuleFor(query => query.Status)
            .Must(status => StatusNames.Contains(status, StringComparer.OrdinalIgnoreCase))
            .When(query => query.Status is not null)
            .WithMessage($"'Status' must be one of: {string.Join(", ", StatusNames)}.");

        RuleFor(query => query.Page)
            .GreaterThanOrEqualTo(1);

        RuleFor(query => query.PageSize)
            .InclusiveBetween(1, 100);
    }
}
