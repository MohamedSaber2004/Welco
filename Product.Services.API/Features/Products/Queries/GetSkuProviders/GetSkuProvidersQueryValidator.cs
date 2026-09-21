using FluentValidation;
using Welco.Shared.Localization;

namespace Product.Services.API.Features.Products.Queries.GetSkuProviders
{
    public class GetSkuProvidersQueryValidator : AbstractValidator<GetSkuProvidersQuery>
    {
        public GetSkuProvidersQueryValidator()
        {
            RuleFor(x => x.Sku)
                .NotEmpty().WithMessage(LocalizationKeys.Product.SkuRequired);

            RuleFor(x => x.PageNumber)
                .GreaterThanOrEqualTo(1).WithMessage(LocalizationKeys.UserManagement.PageNumberPositive);

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 50).WithMessage(LocalizationKeys.UserManagement.PageSizeRange);
        }
    }
}
