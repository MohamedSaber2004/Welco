using FluentValidation;
using Welco.Shared.Localization;

namespace UserManamgent.Service.API.Features.CompanyAddresses.Commands.UpdateCompanyAddress
{
    public class UpdateCompanyAddressCommandValidator : AbstractValidator<UpdateCompanyAddressCommand>
    {
        public UpdateCompanyAddressCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty().WithMessage(LocalizationKeys.CompanyAddress.AddressIdRequired);
            RuleFor(x => x.Street).MaximumLength(250).When(x => x.Street != null);
        }
    }
}
