using FluentValidation;
using Welco.Shared.Localization;

namespace Product.Services.API.Features.Currencies.Queries.GetCurrencies
{
    public class GetCurrenciesQueryValidator : AbstractValidator<GetCurrenciesQuery>
    {
        public GetCurrenciesQueryValidator()
        {
            RuleFor(x => x.PageNumber)
                .GreaterThanOrEqualTo(1).WithMessage(LocalizationKeys.UserManagement.PageNumberPositive);

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 50).WithMessage(LocalizationKeys.UserManagement.PageSizeRange);
        }
    }
}
