using FluentValidation;
using Welco.Shared.Localization;

namespace UserManamgent.Service.API.Features.Companies.Commands.UpdateMyCompany
{
    public class UpdateMyCompanyCommandValidator : AbstractValidator<UpdateMyCompanyCommand>
    {
        public UpdateMyCompanyCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage(LocalizationKeys.Company.NameRequired)
                .MaximumLength(200);

            RuleFor(x => x.Email)
                .EmailAddress().WithMessage(LocalizationKeys.Company.EmailInvalid)
                .MaximumLength(256)
                .When(x => !string.IsNullOrWhiteSpace(x.Email));

            RuleFor(x => x.CountryId)
                .NotEmpty().WithMessage(LocalizationKeys.Company.CountryRequired);
        }
    }
}
