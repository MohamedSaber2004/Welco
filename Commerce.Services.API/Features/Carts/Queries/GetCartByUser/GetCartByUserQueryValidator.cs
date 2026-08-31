using FluentValidation;
using Welco.Shared.Localization;

namespace Commerce.Services.API.Features.Carts.Queries.GetCartByUser
{
    public class GetCartByUserQueryValidator : AbstractValidator<GetCartByUserQuery>
    {
        public GetCartByUserQueryValidator()
        {
            RuleFor(x => x.UserId).NotEmpty().WithMessage(LocalizationKeys.Cart.UserIdRequired);
        }
    }
}
