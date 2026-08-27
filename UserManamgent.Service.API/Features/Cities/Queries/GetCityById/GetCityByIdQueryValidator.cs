using FluentValidation;
using Welco.Shared.Localization;

namespace UserManamgent.Service.API.Features.Cities.Queries.GetCityById
{
    public class GetCityByIdQueryValidator : AbstractValidator<GetCityByIdQuery>
    {
        public GetCityByIdQueryValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage(LocalizationKeys.City.CityIdRequired);
        }
    }
}
