using FluentValidation;
using Welco.Shared.Localization;

namespace Content.Services.API.Features.HelpCategories.Commands.CreateHelpCategory
{
    public class CreateHelpCategoryCommandValidator : AbstractValidator<CreateHelpCategoryCommand>
    {
        public CreateHelpCategoryCommandValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(LocalizationKeys.HelpCategory.NameRequired).MaximumLength(200);
            RuleFor(x => x.Icon).MaximumLength(100).When(x => x.Icon != null);
        }
    }
}
