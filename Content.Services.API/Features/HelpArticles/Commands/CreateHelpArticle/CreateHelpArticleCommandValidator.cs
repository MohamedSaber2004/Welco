using FluentValidation;
using Welco.Shared.Localization;

namespace Content.Services.API.Features.HelpArticles.Commands.CreateHelpArticle
{
    public class CreateHelpArticleCommandValidator : AbstractValidator<CreateHelpArticleCommand>
    {
        public CreateHelpArticleCommandValidator()
        {
            RuleFor(x => x.CategoryId).NotEmpty().WithMessage(LocalizationKeys.HelpArticle.CategoryRequired);
            RuleFor(x => x.Title).NotEmpty().WithMessage(LocalizationKeys.HelpArticle.TitleRequired).MaximumLength(300);
            RuleFor(x => x.Body).NotEmpty().WithMessage(LocalizationKeys.HelpArticle.BodyRequired);
            RuleFor(x => x.Slug).NotEmpty().WithMessage(LocalizationKeys.HelpArticle.SlugRequired).MaximumLength(200);
        }
    }
}
