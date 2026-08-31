using FluentValidation;
using Welco.Shared.Localization;

namespace Content.Services.API.Features.LandingPages.Queries.GetLandingPages
{
    public class GetLandingPagesQueryValidator : AbstractValidator<GetLandingPagesQuery>
    {
        public GetLandingPagesQueryValidator()
        {
            RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1).WithMessage(LocalizationKeys.UserManagement.PageNumberPositive);
            RuleFor(x => x.PageSize).InclusiveBetween(1, 50).WithMessage(LocalizationKeys.UserManagement.PageSizeRange);
        }
    }
}
