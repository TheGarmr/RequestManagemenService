using FluentValidation;

namespace RequestManagementService.Application.Features.ServiceRequests.CreateServiceRequest;

internal sealed class CreateServiceRequestCommandValidator : AbstractValidator<CreateServiceRequestCommand>
{
    public CreateServiceRequestCommandValidator()
    {
        RuleFor(command => command.Title)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(command => command.Description)
            .MaximumLength(2000);

        RuleFor(command => command.Priority)
            .IsInEnum();
    }
}
