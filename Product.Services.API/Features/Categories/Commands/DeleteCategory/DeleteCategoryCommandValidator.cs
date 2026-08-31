using FluentValidation;
using Welco.Shared.Localization;

namespace Product.Services.API.Features.Categories.Commands.DeleteCategory
{
    public class DeleteCategoryCommandValidator : AbstractValidator<DeleteCategoryCommand>
    {
        public DeleteCategoryCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage(LocalizationKeys.Category.CategoryIdRequired);
        }
    }
}
