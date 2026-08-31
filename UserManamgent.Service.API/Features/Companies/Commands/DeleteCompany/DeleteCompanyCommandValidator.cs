using FluentValidation;
using Welco.Shared.Localization;
namespace UserManamgent.Service.API.Features.Companies.Commands.DeleteCompany
{
    public class DeleteCompanyCommandValidator : AbstractValidator<DeleteCompanyCommand>
    {
        public DeleteCompanyCommandValidator() { RuleFor(x => x.Id).NotEmpty().WithMessage(LocalizationKeys.Company.CompanyIdRequired); }
    }
}
