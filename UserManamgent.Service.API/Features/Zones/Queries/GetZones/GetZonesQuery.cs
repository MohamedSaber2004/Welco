using MediatR;
using Welco.Shared.Common.DTOs.UserManagement;
using Welco.Shared.Results;

namespace UserManamgent.Service.API.Features.Zones.Queries.GetZones
{
    public class GetZonesQuery : IRequest<Result<IReadOnlyList<ZoneDto>>>
    {
        public Guid? CityId { get; set; }
    }
}
