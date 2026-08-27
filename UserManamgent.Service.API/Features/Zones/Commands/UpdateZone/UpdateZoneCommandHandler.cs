using MediatR;
using Microsoft.EntityFrameworkCore;
using Welco.Shared.Common.DTOs.UserManagement;
using Welco.Shared.Common.Interfaces;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Domain.Models;
using Welco.Shared.Localization;
using Welco.Shared.Results;

namespace UserManamgent.Service.API.Features.Zones.Commands.UpdateZone
{
    public class UpdateZoneCommandHandler : IRequestHandler<UpdateZoneCommand, Result<ZoneDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public UpdateZoneCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<Result<ZoneDto>> Handle(UpdateZoneCommand request, CancellationToken cancellationToken)
        {
            var zoneRepo = _unitOfWork.GetRepository<Zone, Guid>();
            var zone = await zoneRepo.GetByIdAsync(request.Id, cancellationToken);
            if (zone == null || zone.IsDeleted)
            {
                return Result<ZoneDto>.NotFound(LocalizationKeys.Zone.NotFound);
            }

            var targetCityId = request.CityId ?? zone.CityId;
            if (request.CityId.HasValue)
            {
                var cityRepo = _unitOfWork.GetRepository<City, Guid>();
                var city = await cityRepo.GetByIdAsync(request.CityId.Value, cancellationToken);
                if (city == null || city.IsDeleted)
                {
                    return Result<ZoneDto>.NotFound(LocalizationKeys.City.NotFound);
                }
            }

            var targetNameEn = !string.IsNullOrWhiteSpace(request.NameEn) ? request.NameEn.Trim() : zone.NameEn;
            var targetNameAr = !string.IsNullOrWhiteSpace(request.NameAr) ? request.NameAr.Trim() : zone.NameAr;

            var exists = await zoneRepo.ExistsAsync(
                z => !z.IsDeleted && z.Id != zone.Id && z.CityId == targetCityId &&
                     (z.NameEn.ToLower() == targetNameEn.ToLower() || z.NameAr == targetNameAr),
                cancellationToken);

            if (exists)
            {
                return Result<ZoneDto>.Conflict(LocalizationKeys.Zone.AlreadyExists);
            }

            var currentUserId = _currentUserService.UserId != Guid.Empty
                ? _currentUserService.UserId.ToString()
                : "System";

            zone.Update(request.CityId, request.NameEn, request.NameAr, currentUserId);
            zoneRepo.Update(zone);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var updatedZone = await zoneRepo
                .GetAllWithIncluding(z => z.Id == zone.Id, z => z.City)
                .FirstOrDefaultAsync(cancellationToken);

            var dto = new ZoneDto
            {
                Id = zone.Id,
                CityId = zone.CityId,
                CityNameEn = updatedZone?.City?.NameEn,
                CityNameAr = updatedZone?.City?.NameAr,
                NameEn = zone.NameEn,
                NameAr = zone.NameAr,
                IsActive = zone.IsActive,
                CreatedAt = zone.CreatedAt
            };

            return Result<ZoneDto>.Success(dto, LocalizationKeys.Zone.Updated);
        }
    }
}
