using FluentValidation;
using Welco.Shared.Localization;

namespace UserManamgent.Service.API.Features.Countries.Queries.GetCountryById
{
    public class GetCountryByIdQueryValidator : AbstractValidator<GetCountryByIdQuery>
    {
        public GetCountryByIdQueryValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage(LocalizationKeys.Country.CountryIdRequired);
        }
    }
}
