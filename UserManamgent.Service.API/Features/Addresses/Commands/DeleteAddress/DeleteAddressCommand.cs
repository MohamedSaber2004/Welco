using MediatR;
using Welco.Shared.Results;

namespace UserManamgent.Service.API.Features.Addresses.Commands.DeleteAddress
{
    public class DeleteAddressCommand : IRequest<Result<string>>
    {
        public Guid Id { get; set; }
    }
}
