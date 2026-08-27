using FluentValidation;
using Welco.Shared.Localization;

namespace UserManamgent.Service.API.Features.Countries.Commands.DeleteCountry
{
    public class DeleteCountryCommandValidator : AbstractValidator<DeleteCountryCommand>
    {
        public DeleteCountryCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage(LocalizationKeys.Country.CountryIdRequired);
        }
    }
}
