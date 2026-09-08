using FluentValidation;
using Welco.Shared.Localization;

namespace Content.Services.API.Features.LandingPages.Commands.DeleteLandingPage
{
    public class DeleteLandingPageCommandValidator : AbstractValidator<DeleteLandingPageCommand>
    {
        public DeleteLandingPageCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage(LocalizationKeys.LandingPage.LandingPageIdRequired);
        }
    }
}
