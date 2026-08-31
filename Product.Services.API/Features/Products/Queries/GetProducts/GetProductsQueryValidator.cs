using FluentValidation;
using Welco.Shared.Localization;

namespace Product.Services.API.Features.Products.Queries.GetProducts
{
    public class GetProductsQueryValidator : AbstractValidator<GetProductsQuery>
    {
        public GetProductsQueryValidator()
        {
            RuleFor(x => x.PageNumber)
                .GreaterThanOrEqualTo(1).WithMessage(LocalizationKeys.UserManagement.PageNumberPositive);

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 50).WithMessage(LocalizationKeys.UserManagement.PageSizeRange);
        }
    }
}
