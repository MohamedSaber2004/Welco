using FluentValidation;
using Welco.Shared.Localization;
namespace UserManamgent.Service.API.Features.Companies.Commands.CreateCompany
{
    public class CreateCompanyCommandValidator : AbstractValidator<CreateCompanyCommand>
    {
        public CreateCompanyCommandValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(LocalizationKeys.Company.NameRequired).MaximumLength(200);
            RuleFor(x => x.Type).IsInEnum().WithMessage(LocalizationKeys.Company.TypeRequired);
            RuleFor(x => x.CountryId).NotEmpty().WithMessage(LocalizationKeys.Company.CountryRequired);
            RuleFor(x => x.TierLevel).InclusiveBetween(1,5).WithMessage(LocalizationKeys.Company.TierLevelInvalid);
        }
    }
}
