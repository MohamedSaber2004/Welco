using FluentValidation;
using Welco.Shared.Localization;

namespace Product.Services.API.Features.Categories.Commands.UpdateCategory
{
    public class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
    {
        public UpdateCategoryCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage(LocalizationKeys.Category.CategoryIdRequired);

            RuleFor(x => x.NameEn)
                .NotEmpty().WithMessage(LocalizationKeys.Category.NameEnRequired)
                .MaximumLength(200);

            RuleFor(x => x.NameAr)
                .NotEmpty().WithMessage(LocalizationKeys.Category.NameArRequired)
                .MaximumLength(200);

            RuleFor(x => x.Description)
                .MaximumLength(1000)
                .When(x => x.Description != null);

            RuleFor(x => x.ImageName)
                .MaximumLength(500)
                .When(x => x.ImageName != null);
        }
    }
}
