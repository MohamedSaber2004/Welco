using FluentValidation;
using Welco.Shared.Localization;

namespace UserManamgent.Service.API.Features.Addresses.Queries.GetUserAddresses
{
    public class GetUserAddressesQueryValidator : AbstractValidator<GetUserAddressesQuery>
    {
        public GetUserAddressesQueryValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage(LocalizationKeys.UserAddress.UserIdRequired);
        }
    }
}
