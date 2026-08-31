using FluentValidation;
using Welco.Shared.Localization;

namespace Certification.Services.API.Features.Certifications.Commands.UpdateCertification
{
    public class UpdateCertificationCommandValidator : AbstractValidator<UpdateCertificationCommand>
    {
        public UpdateCertificationCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage(LocalizationKeys.Certification.CertificationIdRequired);

            RuleFor(x => x.CertificateNumber)
                .NotEmpty().WithMessage(LocalizationKeys.Certification.CertificateNumberRequired)
                .MaximumLength(50);

            RuleFor(x => x.Title)
                .NotEmpty().WithMessage(LocalizationKeys.Certification.TitleRequired)
                .MaximumLength(200);

            RuleFor(x => x.IssuedTo)
                .NotEmpty().WithMessage(LocalizationKeys.Certification.IssuedToRequired)
                .MaximumLength(200);

            RuleFor(x => x.Issuer)
                .NotEmpty().WithMessage(LocalizationKeys.Certification.IssuerRequired)
                .MaximumLength(200);

            RuleFor(x => x.IssueDate)
                .NotEmpty().WithMessage(LocalizationKeys.Certification.IssueDateRequired)
                .LessThanOrEqualTo(DateTime.UtcNow.Date).WithMessage(LocalizationKeys.Certification.IssueDateInFuture);

            RuleFor(x => x.ExpiryDate)
                .GreaterThan(x => x.IssueDate).WithMessage(LocalizationKeys.Certification.ExpiryDateBeforeIssueDate)
                .When(x => x.ExpiryDate.HasValue);

            RuleFor(x => x.Description)
                .MaximumLength(1000)
                .When(x => x.Description != null);

            RuleFor(x => x.CertificationImageName)
                .MaximumLength(500)
                .When(x => x.CertificationImageName != null);
        }
    }
}
