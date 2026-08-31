using FluentValidation;
using Welco.Shared.Localization;

namespace Commerce.Services.API.Features.Orders.Commands.UpdateOrderStatus
{
    public class UpdateOrderStatusCommandValidator : AbstractValidator<UpdateOrderStatusCommand>
    {
        public UpdateOrderStatusCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty().WithMessage(LocalizationKeys.Order.OrderIdRequired);
            RuleFor(x => x.Status).NotEmpty().WithMessage(LocalizationKeys.Order.StatusRequired);
        }
    }
}
