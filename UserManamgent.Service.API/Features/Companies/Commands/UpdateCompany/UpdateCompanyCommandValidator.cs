using FluentValidation;
using Welco.Shared.Localization;
namespace UserManamgent.Service.API.Features.Companies.Commands.UpdateCompany
{
    public class UpdateCompanyCommandValidator : AbstractValidator<UpdateCompanyCommand>
    {
        public UpdateCompanyCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty().WithMessage(LocalizationKeys.Company.CompanyIdRequired);
            RuleFor(x => x.Name).NotEmpty().WithMessage(LocalizationKeys.Company.NameRequired).MaximumLength(200);
            RuleFor(x => x.Email).EmailAddress().WithMessage(LocalizationKeys.Company.EmailInvalid).MaximumLength(256).When(x => !string.IsNullOrWhiteSpace(x.Email));
            RuleFor(x => x.Type).IsInEnum().WithMessage(LocalizationKeys.Company.TypeRequired);
            RuleFor(x => x.CountryId).NotEmpty().WithMessage(LocalizationKeys.Company.CountryRequired);
        }
    }
}
