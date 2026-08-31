using FluentValidation;
using Welco.Shared.Localization;

namespace Commerce.Services.API.Features.Carts.Commands.CreateCart
{
    public class CreateCartCommandValidator : AbstractValidator<CreateCartCommand>
    {
        public CreateCartCommandValidator()
        {
            RuleFor(x => x).Must(x => x.UserId.HasValue || !string.IsNullOrWhiteSpace(x.SessionId))
                .WithMessage(LocalizationKeys.Cart.UserIdOrSessionRequired);
            RuleFor(x => x.SessionId).MaximumLength(200).When(x => x.SessionId != null);
        }
    }
}
