using FluentValidation;
using Welco.Shared.Localization;

namespace Content.Services.API.Features.HelpArticles.Commands.UpdateHelpArticle
{
    public class UpdateHelpArticleCommandValidator : AbstractValidator<UpdateHelpArticleCommand>
    {
        public UpdateHelpArticleCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty().WithMessage(LocalizationKeys.HelpArticle.HelpArticleIdRequired);
            RuleFor(x => x.CategoryId).NotEmpty().WithMessage(LocalizationKeys.HelpArticle.CategoryRequired);
            RuleFor(x => x.Title).NotEmpty().WithMessage(LocalizationKeys.HelpArticle.TitleRequired).MaximumLength(300);
            RuleFor(x => x.Body).NotEmpty().WithMessage(LocalizationKeys.HelpArticle.BodyRequired);
            RuleFor(x => x.Slug).NotEmpty().WithMessage(LocalizationKeys.HelpArticle.SlugRequired).MaximumLength(200);
        }
    }
}
