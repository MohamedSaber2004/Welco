using FluentValidation;
using Welco.Shared.Localization;

namespace Commerce.Services.API.Features.Orders.Commands.CreateOrder
{
    public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
    {
        public CreateOrderCommandValidator()
        {
            RuleFor(x => x.Items).NotEmpty().WithMessage(LocalizationKeys.Order.ItemsRequired);
            RuleForEach(x => x.Items).ChildRules(i =>
            {
                i.RuleFor(v => v.ProductId).NotEmpty().WithMessage(LocalizationKeys.Product.ProductIdRequired);
                i.RuleFor(v => v.Quantity).GreaterThan(0).WithMessage(LocalizationKeys.Order.QuantityPositive);
                i.RuleFor(v => v.UnitPrice).GreaterThanOrEqualTo(0).WithMessage(LocalizationKeys.Order.PriceNotNegative);
            });
        }
    }
}
