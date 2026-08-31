using FluentValidation;
using Welco.Shared.Localization;
namespace UserManamgent.Service.API.Features.Companies.Queries.GetCompanyById
{
    public class GetCompanyByIdQueryValidator : AbstractValidator<GetCompanyByIdQuery>
    {
        public GetCompanyByIdQueryValidator() { RuleFor(x => x.Id).NotEmpty().WithMessage(LocalizationKeys.Company.CompanyIdRequired); }
    }
}
