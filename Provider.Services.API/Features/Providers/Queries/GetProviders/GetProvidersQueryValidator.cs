using FluentValidation;
using Welco.Shared.Localization;

namespace Provider.Services.API.Features.Providers.Queries.GetProviders
{
    public class GetProvidersQueryValidator : AbstractValidator<GetProvidersQuery>
    {
        public GetProvidersQueryValidator()
        {
            RuleFor(x => x.PageNumber)
                .GreaterThanOrEqualTo(1).WithMessage(LocalizationKeys.UserManagement.PageNumberPositive);

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 50).WithMessage(LocalizationKeys.UserManagement.PageSizeRange);
        }
    }
}
