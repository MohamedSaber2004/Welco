using FluentValidation;
using Welco.Shared.Localization;

namespace UserManamgent.Service.API.Features.Addresses.Commands.CreateAddress
{
    public class CreateAddressCommandValidator : AbstractValidator<CreateAddressCommand>
    {
        public CreateAddressCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage(LocalizationKeys.UserAddress.UserIdRequired);

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
