using FluentValidation;

namespace Commerce.Services.API.Features.Integration.Orders.Commands.CreateExternalOrder
{
    public class CreateExternalOrderCommandValidator : AbstractValidator<CreateExternalOrderCommand>
    {
        public CreateExternalOrderCommandValidator()
        {
            RuleFor(x => x.Items)
                .NotEmpty().WithMessage("Order must contain at least one item.");

            RuleForEach(x => x.Items).ChildRules(item =>
            {
                item.RuleFor(i => i.WelcoProductId).NotEmpty().WithMessage("WelcoProductId is required.");
                item.RuleFor(i => i.Quantity).GreaterThan(0).WithMessage("Quantity must be greater than zero.");
                item.RuleFor(i => i.UnitPrice).GreaterThanOrEqualTo(0).WithMessage("UnitPrice cannot be negative.");
            });
        }
    }
}
