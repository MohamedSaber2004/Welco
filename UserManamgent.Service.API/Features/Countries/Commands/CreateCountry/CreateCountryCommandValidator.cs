using FluentValidation;
using Welco.Shared.Localization;

namespace UserManamgent.Service.API.Features.Countries.Commands.CreateCountry
{
    public class CreateCountryCommandValidator : AbstractValidator<CreateCountryCommand>
    {
        public CreateCountryCommandValidator()
        {
            RuleFor(x => x.NameEn)
                .NotEmpty().WithMessage(LocalizationKeys.Country.NameEnRequired)
                .MaximumLength(150);

            RuleFor(x => x.NameAr)
                .NotEmpty().WithMessage(LocalizationKeys.Country.NameArRequired)
                .MaximumLength(150);

            RuleFor(x => x.Code)
                .MaximumLength(10);
        }
    }
}
