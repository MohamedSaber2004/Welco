using FluentValidation;
using Welco.Shared.Localization;

namespace Provider.Services.API.Features.Providers.Commands.UpdateProvider
{
    public class UpdateProviderCommandValidator : AbstractValidator<UpdateProviderCommand>
    {
        public UpdateProviderCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage(LocalizationKeys.Provider.ProviderIdRequired);

            RuleFor(x => x.CommercialName)
                .NotEmpty().WithMessage(LocalizationKeys.Provider.CommercialNameRequired)
                .MaximumLength(200)
                .When(x => x.CommercialName != null);

            RuleFor(x => x.Email)
                .EmailAddress().WithMessage(LocalizationKeys.Provider.EmailInvalid)
                .MaximumLength(150)
                .When(x => x.Email != null);

            RuleFor(x => x.CompanyName)
                .MaximumLength(200)
                .When(x => x.CompanyName != null);

            RuleFor(x => x.CommercialRegistrationNumber)
                .MaximumLength(50)
                .When(x => x.CommercialRegistrationNumber != null);

            RuleFor(x => x.ContactPersonName)
                .MaximumLength(150)
                .When(x => x.ContactPersonName != null);

            RuleFor(x => x.ContactPersonPhone)
                .MaximumLength(30)
                .When(x => x.ContactPersonPhone != null);

            RuleFor(x => x.Phone)
                .MaximumLength(30)
                .When(x => x.Phone != null);

            RuleFor(x => x.Address)
                .MaximumLength(500)
                .When(x => x.Address != null);

            RuleFor(x => x.Notes)
                .MaximumLength(1000)
                .When(x => x.Notes != null);

            RuleFor(x => x.ImageName)
                .MaximumLength(500)
                .When(x => x.ImageName != null);
        }
    }
}
