using FluentValidation;
using Welco.Shared.Localization;

namespace Certification.Services.API.Features.Certifications.Queries.ShowCertification
{
    public class ShowCertificationQueryValidator : AbstractValidator<ShowCertificationQuery>
    {
        public ShowCertificationQueryValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage(LocalizationKeys.Certification.CertificationIdRequired);
        }
    }
}
