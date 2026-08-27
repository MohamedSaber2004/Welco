using FluentValidation;
using Welco.Shared.Localization;

namespace UserManamgent.Service.API.Features.Cities.Commands.DeleteCity
{
    public class DeleteCityCommandValidator : AbstractValidator<DeleteCityCommand>
    {
        public DeleteCityCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage(LocalizationKeys.City.CityIdRequired);
        }
    }
}
