using FluentValidation;
using Welco.Shared.Localization;

namespace Commerce.Services.API.Features.Carts.Commands.RemoveCartItem
{
    public class RemoveCartItemCommandValidator : AbstractValidator<RemoveCartItemCommand>
    {
        public RemoveCartItemCommandValidator()
        {
            RuleFor(x => x.CartId).NotEmpty().WithMessage(LocalizationKeys.Cart.CartIdRequired);
            RuleFor(x => x.ItemId).NotEmpty().WithMessage(LocalizationKeys.Cart.CartItemIdRequired);
        }
    }
}
