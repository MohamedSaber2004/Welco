using FluentValidation;
using Welco.Shared.Localization;

namespace UserManamgent.Service.API.Features.Addresses.Commands.UpdateAddress
{
    public class UpdateAddressCommandValidator : AbstractValidator<UpdateAddressCommand>
    {
        public UpdateAddressCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage(LocalizationKeys.UserAddress.AddressIdRequired);

            RuleFor(x => x.Street)
                .MaximumLength(250)
                .When(x => !string.IsNullOrEmpty(x.Street));
        }
    }
}
