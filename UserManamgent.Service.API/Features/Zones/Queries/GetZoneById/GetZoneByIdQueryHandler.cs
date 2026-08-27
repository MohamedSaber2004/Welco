using MediatR;
using Microsoft.EntityFrameworkCore;
using Welco.Shared.Common.DTOs.UserManagement;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Domain.Models;
using Welco.Shared.Localization;
using Welco.Shared.Results;

namespace UserManamgent.Service.API.Features.Zones.Queries.GetZoneById
{
    public class GetZoneByIdQueryHandler : IRequestHandler<GetZoneByIdQuery, Result<ZoneDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetZoneByIdQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<ZoneDto>> Handle(GetZoneByIdQuery request, CancellationToken cancellationToken)
        {
            var zoneRepo = _unitOfWork.GetRepository<Zone, Guid>();
            var zone = await zoneRepo
                .GetAllWithIncluding(z => z.Id == request.Id && !z.IsDeleted, z => z.City)
                .FirstOrDefaultAsync(cancellationToken);

            if (zone == null)
            {
                return Result<ZoneDto>.NotFound(LocalizationKeys.Zone.NotFound);
            }

            var dto = new ZoneDto
            {
                Id = zone.Id,
                CityId = zone.CityId,
                CityNameEn = zone.City?.NameEn,
                CityNameAr = zone.City?.NameAr,
                NameEn = zone.NameEn,
                NameAr = zone.NameAr,
                IsActive = zone.IsActive,
                CreatedAt = zone.CreatedAt
            };

            return Result<ZoneDto>.Success(dto, LocalizationKeys.Zone.Fetched);
        }
    }
}
