using FluentValidation;
using Welco.Shared.Localization;

namespace Product.Services.API.Features.Categories.Queries.GetCategoryProviders
{
    public class GetCategoryProvidersQueryValidator : AbstractValidator<GetCategoryProvidersQuery>
    {
        public GetCategoryProvidersQueryValidator()
        {
            RuleFor(x => x.PageNumber)
                .GreaterThanOrEqualTo(1).WithMessage(LocalizationKeys.UserManagement.PageNumberPositive);

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 50).WithMessage(LocalizationKeys.UserManagement.PageSizeRange);
        }
    }
}
