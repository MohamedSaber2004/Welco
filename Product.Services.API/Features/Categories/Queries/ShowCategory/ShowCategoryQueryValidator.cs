using FluentValidation;
using Welco.Shared.Localization;

namespace Product.Services.API.Features.Categories.Queries.ShowCategory
{
    public class ShowCategoryQueryValidator : AbstractValidator<ShowCategoryQuery>
    {
        public ShowCategoryQueryValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage(LocalizationKeys.Category.CategoryIdRequired);
        }
    }
}
