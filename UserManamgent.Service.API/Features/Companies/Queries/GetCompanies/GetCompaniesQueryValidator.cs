using FluentValidation;
using Welco.Shared.Localization;
namespace UserManamgent.Service.API.Features.Companies.Queries.GetCompanies
{
    public class GetCompaniesQueryValidator : AbstractValidator<GetCompaniesQuery>
    {
        public GetCompaniesQueryValidator()
        {
            RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1).WithMessage(LocalizationKeys.UserManagement.PageNumberPositive);
            RuleFor(x => x.PageSize).InclusiveBetween(1, 50).WithMessage(LocalizationKeys.UserManagement.PageSizeRange);
        }
    }
}
