using FluentValidation;
using Welco.Shared.Localization;

namespace Content.Services.API.Features.SupportContact.Commands.UpdateSupportContact
{
    public class UpdateSupportContactCommandValidator : AbstractValidator<UpdateSupportContactCommand>
    {
        public UpdateSupportContactCommandValidator()
        {
            RuleFor(x => x.SupportEmail)
                .NotEmpty().WithMessage(LocalizationKeys.SupportContact.EmailRequired)
                .EmailAddress().WithMessage(LocalizationKeys.SupportContact.EmailInvalid);

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage(LocalizationKeys.SupportContact.PhoneRequired);

            RuleFor(x => x.WhatsAppNumber)
                .NotEmpty().WithMessage(LocalizationKeys.SupportContact.WhatsAppRequired);
        }
    }
}
