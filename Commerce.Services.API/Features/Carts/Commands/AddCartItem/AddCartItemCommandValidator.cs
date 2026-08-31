using FluentValidation;
using Welco.Shared.Localization;

namespace Commerce.Services.API.Features.Carts.Commands.AddCartItem
{
    public class AddCartItemCommandValidator : AbstractValidator<AddCartItemCommand>
    {
        public AddCartItemCommandValidator()
        {
            RuleFor(x => x.CartId).NotEmpty().WithMessage(LocalizationKeys.Cart.CartIdRequired);
            RuleFor(x => x.ProductId).NotEmpty().WithMessage(LocalizationKeys.Product.ProductIdRequired);
            RuleFor(x => x.Quantity).GreaterThan(0).WithMessage(LocalizationKeys.Cart.QuantityPositive);
            RuleFor(x => x.UnitPriceSnapshot).GreaterThanOrEqualTo(0).WithMessage(LocalizationKeys.Cart.PriceNotNegative);
        }
    }
}
