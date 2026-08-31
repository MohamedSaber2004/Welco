using FluentValidation;
using Welco.Shared.Localization;

namespace Commerce.Services.API.Features.Orders.Queries.GetOrderById
{
    public class GetOrderByIdQueryValidator : AbstractValidator<GetOrderByIdQuery>
    {
        public GetOrderByIdQueryValidator()
        {
            RuleFor(x => x.Id).NotEmpty().WithMessage(LocalizationKeys.Order.OrderIdRequired);
        }
    }
}
