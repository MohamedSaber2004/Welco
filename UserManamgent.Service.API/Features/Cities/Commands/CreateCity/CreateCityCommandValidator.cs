using FluentValidation;
using Welco.Shared.Localization;

namespace UserManamgent.Service.API.Features.Cities.Commands.CreateCity
{
    public class CreateCityCommandValidator : AbstractValidator<CreateCityCommand>
    {
        public CreateCityCommandValidator()
        {
            RuleFor(x => x.CountryId)
                .NotEmpty().WithMessage(LocalizationKeys.City.CountryIdRequired);

            RuleFor(x => x.NameEn)
                .NotEmpty().WithMessage(LocalizationKeys.City.NameEnRequired)
                .MaximumLength(150);

            RuleFor(x => x.NameAr)
                .NotEmpty().WithMessage(LocalizationKeys.City.NameArRequired)
                .MaximumLength(150);
        }
    }
}
