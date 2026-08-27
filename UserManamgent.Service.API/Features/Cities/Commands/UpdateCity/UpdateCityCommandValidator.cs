using FluentValidation;
using Welco.Shared.Localization;

namespace UserManamgent.Service.API.Features.Cities.Commands.UpdateCity
{
    public class UpdateCityCommandValidator : AbstractValidator<UpdateCityCommand>
    {
        public UpdateCityCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage(LocalizationKeys.City.CityIdRequired);

            RuleFor(x => x.NameEn)
                .MaximumLength(150)
                .When(x => !string.IsNullOrEmpty(x.NameEn));

            RuleFor(x => x.NameAr)
                .MaximumLength(150)
                .When(x => !string.IsNullOrEmpty(x.NameAr));
        }
    }
}
