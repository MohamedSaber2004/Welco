using FluentValidation;
using Welco.Shared.Localization;

namespace UserManamgent.Service.API.Features.Addresses.Queries.GetAddressById
{
    public class GetAddressByIdQueryValidator : AbstractValidator<GetAddressByIdQuery>
    {
        public GetAddressByIdQueryValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage(LocalizationKeys.UserAddress.AddressIdRequired);
        }
    }
}
