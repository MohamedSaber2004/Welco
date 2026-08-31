using FluentValidation;
using Welco.Shared.Localization;

namespace Content.Services.API.Features.LandingPages.Queries.GetLandingPageBySlug
{
    public class GetLandingPageBySlugQueryValidator : AbstractValidator<GetLandingPageBySlugQuery>
    {
        public GetLandingPageBySlugQueryValidator()
        {
            RuleFor(x => x.Slug).NotEmpty().WithMessage(LocalizationKeys.LandingPage.SlugRequired);
        }
    }
}
