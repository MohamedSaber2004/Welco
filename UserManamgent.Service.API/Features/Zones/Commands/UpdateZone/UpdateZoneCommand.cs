using MediatR;
using Welco.Shared.Common.DTOs.UserManagement;
using Welco.Shared.Results;

namespace UserManamgent.Service.API.Features.Zones.Commands.UpdateZone
{
    public class UpdateZoneCommand : IRequest<Result<ZoneDto>>
    {
        public Guid Id { get; set; }
        public Guid? CityId { get; set; }
        public string? NameEn { get; set; }
        public string? NameAr { get; set; }
    }
}
