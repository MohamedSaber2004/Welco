using FluentValidation;
using Welco.Shared.Localization;

namespace Certification.Services.API.Features.Certifications.Queries.GetCertifications
{
    public class GetCertificationsQueryValidator : AbstractValidator<GetCertificationsQuery>
    {
        public GetCertificationsQueryValidator()
        {
            RuleFor(x => x.PageNumber)
                .GreaterThanOrEqualTo(1).WithMessage(LocalizationKeys.UserManagement.PageNumberPositive);

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 50).WithMessage(LocalizationKeys.UserManagement.PageSizeRange);
        }
    }
}
