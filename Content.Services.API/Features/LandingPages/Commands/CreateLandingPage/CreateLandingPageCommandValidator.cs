using FluentValidation;
using Welco.Shared.Localization;

namespace Content.Services.API.Features.LandingPages.Commands.CreateLandingPage
{
    public class CreateLandingPageCommandValidator : AbstractValidator<CreateLandingPageCommand>
    {
        public CreateLandingPageCommandValidator()
        {
            RuleFor(x => x.Type).NotEmpty().WithMessage(LocalizationKeys.LandingPage.TypeRequired).MaximumLength(50);
            RuleFor(x => x.Slug).NotEmpty().WithMessage(LocalizationKeys.LandingPage.SlugRequired).MaximumLength(200);
            RuleFor(x => x.HeroTitle).NotEmpty().WithMessage(LocalizationKeys.LandingPage.HeroTitleRequired).MaximumLength(300);
            RuleFor(x => x.HeroBody).MaximumLength(2000).When(x => x.HeroBody != null);
            RuleFor(x => x.ContentBlock).MaximumLength(4000).When(x => x.ContentBlock != null);
        }
    }
}
