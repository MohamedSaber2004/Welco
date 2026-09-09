using FluentValidation;
using Welco.Shared.Localization;
namespace UserManamgent.Service.API.Features.Companies.Commands.CreateCompany
{
    public class CreateCompanyCommandValidator : AbstractValidator<CreateCompanyCommand>
    {
        public CreateCompanyCommandValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(LocalizationKeys.Company.NameRequired).MaximumLength(200);
            RuleFor(x => x.Email).EmailAddress().WithMessage(LocalizationKeys.Company.EmailInvalid).MaximumLength(256).When(x => !string.IsNullOrWhiteSpace(x.Email));
            RuleFor(x => x.Type).IsInEnum().WithMessage(LocalizationKeys.Company.TypeRequired);
            RuleFor(x => x.CountryId).NotEmpty().WithMessage(LocalizationKeys.Company.CountryRequired);
        }
    }
}
