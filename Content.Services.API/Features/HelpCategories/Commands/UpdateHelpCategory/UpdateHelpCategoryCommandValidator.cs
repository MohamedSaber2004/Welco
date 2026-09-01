using FluentValidation;
using Welco.Shared.Localization;

namespace Content.Services.API.Features.HelpCategories.Commands.UpdateHelpCategory
{
    public class UpdateHelpCategoryCommandValidator : AbstractValidator<UpdateHelpCategoryCommand>
    {
        public UpdateHelpCategoryCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty().WithMessage(LocalizationKeys.HelpCategory.HelpCategoryIdRequired);
            RuleFor(x => x.Name).NotEmpty().WithMessage(LocalizationKeys.HelpCategory.NameRequired).MaximumLength(200);
            RuleFor(x => x.Icon).MaximumLength(100).When(x => x.Icon != null);
        }
    }
}
