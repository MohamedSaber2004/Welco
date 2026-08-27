using MediatR;
using Welco.Shared.Results;

namespace UserManamgent.Service.API.Features.Zones.Commands.DeleteZone
{
    public class DeleteZoneCommand : IRequest<Result<string>>
    {
        public Guid Id { get; set; }
    }
}
