using MediatR;
using Welco.Shared.Common.DTOs.UserManagement;
using Welco.Shared.Results;

namespace UserManamgent.Service.API.Features.Addresses.Queries.GetUserAddresses
{
    public class GetUserAddressesQuery : IRequest<Result<IReadOnlyList<UserAddressDto>>>
    {
        public Guid UserId { get; set; }
    }
}
