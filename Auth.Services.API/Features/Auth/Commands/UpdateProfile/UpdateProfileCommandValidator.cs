using FluentValidation;
using Welco.Shared.Common.DTOs.Auth.Requests;
using Welco.Shared.Localization;

namespace Auth.Services.API.Features.Auth.Commands.UpdateProfile
{
    public class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
    {
        public UpdateProfileCommandValidator()
        {
            RuleFor(x => x.FullName)
                .MaximumLength(150)
                .When(x => !string.IsNullOrEmpty(x.FullName));

            RuleFor(x => x.PhoneNumber)
                .MaximumLength(20)
                .When(x => !string.IsNullOrEmpty(x.PhoneNumber));

            RuleForEach(x => x.Addresses)
                .SetValidator(new UpdateProfileAddressDtoValidator())
                .When(x => x.Addresses != null);
        }
    }

    public class UpdateProfileAddressDtoValidator : AbstractValidator<UpdateProfileAddressDto>
    {
        public UpdateProfileAddressDtoValidator()
        {
            RuleFor(x => x.CountryId)
                .NotEmpty().WithMessage(LocalizationKeys.UserAddress.CountryIdRequired);

            RuleFor(x => x.CityId)
                .NotEmpty().WithMessage(LocalizationKeys.UserAddress.CityIdRequired);

            RuleFor(x => x.ZoneId)
                .NotEmpty().WithMessage(LocalizationKeys.UserAddress.ZoneIdRequired);

            RuleFor(x => x.Street)
                .NotEmpty().WithMessage(LocalizationKeys.UserAddress.StreetRequired)
                .MaximumLength(250);

            RuleFor(x => x.Building)
                .MaximumLength(100);

            RuleFor(x => x.Floor)
                .MaximumLength(50);

            RuleFor(x => x.Apartment)
                .MaximumLength(50);
        }
    }
}
