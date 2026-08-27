using MediatR;
using Welco.Shared.Common.DTOs.UserManagement;
using Welco.Shared.Results;

namespace UserManamgent.Service.API.Features.Zones.Queries.GetZoneById
{
    public class GetZoneByIdQuery : IRequest<Result<ZoneDto>>
    {
        public Guid Id { get; set; }
    }
}
