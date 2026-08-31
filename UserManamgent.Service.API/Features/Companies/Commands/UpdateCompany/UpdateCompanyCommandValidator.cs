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
            RuleFor(x => x.Type).IsInEnum().WithMessage(LocalizationKeys.Company.TypeRequired);
            RuleFor(x => x.CountryId).NotEmpty().WithMessage(LocalizationKeys.Company.CountryRequired);
            RuleFor(x => x.TierLevel).InclusiveBetween(1,5).WithMessage(LocalizationKeys.Company.TierLevelInvalid);
        }
    }
}
