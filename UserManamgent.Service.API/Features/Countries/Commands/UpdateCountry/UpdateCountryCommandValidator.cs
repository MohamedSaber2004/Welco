using FluentValidation;
using Welco.Shared.Localization;

namespace UserManamgent.Service.API.Features.Countries.Commands.UpdateCountry
{
    public class UpdateCountryCommandValidator : AbstractValidator<UpdateCountryCommand>
    {
        public UpdateCountryCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage(LocalizationKeys.Country.CountryIdRequired);

            RuleFor(x => x.NameEn)
                .MaximumLength(150)
                .When(x => !string.IsNullOrEmpty(x.NameEn));

            RuleFor(x => x.NameAr)
                .MaximumLength(150)
                .When(x => !string.IsNullOrEmpty(x.NameAr));

            RuleFor(x => x.Code)
                .MaximumLength(10)
                .When(x => !string.IsNullOrEmpty(x.Code));
        }
    }
}
