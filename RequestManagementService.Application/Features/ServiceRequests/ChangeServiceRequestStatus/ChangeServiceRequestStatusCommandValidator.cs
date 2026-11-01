using FluentValidation;

namespace RequestManagementService.Application.Features.ServiceRequests.ChangeServiceRequestStatus;

internal sealed class ChangeServiceRequestStatusCommandValidator : AbstractValidator<ChangeServiceRequestStatusCommand>
{
    public ChangeServiceRequestStatusCommandValidator()
    {
        RuleFor(command => command.Status)
            .NotNull()
            .IsInEnum();
    }
}
