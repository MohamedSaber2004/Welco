using FluentValidation;
using Welco.Shared.Localization;

namespace Commerce.Services.API.Features.Carts.Queries.GetCartBySession
{
    public class GetCartBySessionQueryValidator : AbstractValidator<GetCartBySessionQuery>
    {
        public GetCartBySessionQueryValidator()
        {
            RuleFor(x => x.SessionId).NotEmpty().WithMessage(LocalizationKeys.Cart.SessionIdRequired);
        }
    }
}
