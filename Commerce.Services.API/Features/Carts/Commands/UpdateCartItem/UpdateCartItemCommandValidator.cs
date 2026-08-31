using FluentValidation;
using Welco.Shared.Localization;

namespace Commerce.Services.API.Features.Carts.Commands.UpdateCartItem
{
    public class UpdateCartItemCommandValidator : AbstractValidator<UpdateCartItemCommand>
    {
        public UpdateCartItemCommandValidator()
        {
            RuleFor(x => x.CartId).NotEmpty().WithMessage(LocalizationKeys.Cart.CartIdRequired);
            RuleFor(x => x.ItemId).NotEmpty().WithMessage(LocalizationKeys.Cart.CartItemIdRequired);
            RuleFor(x => x.Quantity).GreaterThan(0).WithMessage(LocalizationKeys.Cart.QuantityPositive);
        }
    }
}
