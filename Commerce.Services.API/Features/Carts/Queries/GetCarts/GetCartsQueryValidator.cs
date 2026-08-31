using FluentValidation;
using Welco.Shared.Localization;

namespace Commerce.Services.API.Features.Carts.Queries.GetCarts
{
    public class GetCartsQueryValidator : AbstractValidator<GetCartsQuery>
    {
        public GetCartsQueryValidator()
        {
            RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1).WithMessage(LocalizationKeys.UserManagement.PageNumberPositive);
            RuleFor(x => x.PageSize).InclusiveBetween(1, 50).WithMessage(LocalizationKeys.UserManagement.PageSizeRange);
        }
    }
}
