using MediatR;
using Welco.Shared.Common.DTOs.UserManagement;
using Welco.Shared.Results;

namespace UserManamgent.Service.API.Features.Addresses.Queries.GetAddressById
{
    public class GetAddressByIdQuery : IRequest<Result<UserAddressDto>>
    {
        public Guid Id { get; set; }
    }
}
