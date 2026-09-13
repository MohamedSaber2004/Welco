using FluentValidation;

namespace Auth.Services.API.Features.Integration.Token
{
    public class CreateIntegrationTokenCommandValidator : AbstractValidator<CreateIntegrationTokenCommand>
    {
        public CreateIntegrationTokenCommandValidator()
        {
            RuleFor(x => x.ClientId)
                .NotEmpty().WithMessage("ClientId is required.");

            RuleFor(x => x.ClientSecret)
                .NotEmpty().WithMessage("ClientSecret is required.");
        }
    }
}
