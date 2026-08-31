using FluentValidation;
using Welco.Shared.Localization;

namespace Certification.Services.API.Features.Certifications.Commands.DeleteCertification
{
    public class DeleteCertificationCommandValidator : AbstractValidator<DeleteCertificationCommand>
    {
        public DeleteCertificationCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage(LocalizationKeys.Certification.CertificationIdRequired);
        }
    }
}
