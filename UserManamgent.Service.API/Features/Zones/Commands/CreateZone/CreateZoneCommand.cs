using MediatR;
using Welco.Shared.Common.DTOs.UserManagement;
using Welco.Shared.Results;

namespace UserManamgent.Service.API.Features.Zones.Commands.CreateZone
{
    public class CreateZoneCommand : IRequest<Result<ZoneDto>>
    {
        public Guid CityId { get; set; }
        public string NameEn { get; set; } = string.Empty;
        public string NameAr { get; set; } = string.Empty;
    }
}
