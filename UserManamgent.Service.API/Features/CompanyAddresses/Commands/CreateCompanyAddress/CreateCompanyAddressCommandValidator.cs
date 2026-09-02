using FluentValidation;
using Welco.Shared.Localization;

namespace UserManamgent.Service.API.Features.CompanyAddresses.Commands.CreateCompanyAddress
{
    public class CreateCompanyAddressCommandValidator : AbstractValidator<CreateCompanyAddressCommand>
    {
        public CreateCompanyAddressCommandValidator()
        {
            RuleFor(x => x.CompanyId)
                .NotEmpty().WithMessage(LocalizationKeys.Company.NotFound);

            RuleFor(x => x.CountryId)
                .NotEmpty().WithMessage(LocalizationKeys.UserAddress.CountryIdRequired);

            RuleFor(x => x.CityId)
                .NotEmpty().WithMessage(LocalizationKeys.UserAddress.CityIdRequired);

            RuleFor(x => x.ZoneId)
                .NotEmpty().WithMessage(LocalizationKeys.UserAddress.ZoneIdRequired);

            RuleFor(x => x.Street)
                .NotEmpty().WithMessage(LocalizationKeys.UserAddress.StreetRequired)
                .MaximumLength(250);
        }
    }
}
