using FluentValidation;
using Welco.Shared.Localization;

namespace Product.Services.API.Features.Categories.Queries.GetCategoryProducts
{
    public class GetCategoryProductsQueryValidator : AbstractValidator<GetCategoryProductsQuery>
    {
        public GetCategoryProductsQueryValidator()
        {
            RuleFor(x => x.CategoryId)
                .NotEmpty().WithMessage(LocalizationKeys.Category.CategoryIdRequired);
        }
    }
}
