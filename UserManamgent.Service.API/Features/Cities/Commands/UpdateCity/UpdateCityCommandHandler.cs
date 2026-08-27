using MediatR;
using Microsoft.EntityFrameworkCore;
using Welco.Shared.Common.DTOs.UserManagement;
using Welco.Shared.Common.Interfaces;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Domain.Models;
using Welco.Shared.Localization;
using Welco.Shared.Results;

namespace UserManamgent.Service.API.Features.Cities.Commands.UpdateCity
{
    public class UpdateCityCommandHandler : IRequestHandler<UpdateCityCommand, Result<CityDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public UpdateCityCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<Result<CityDto>> Handle(UpdateCityCommand request, CancellationToken cancellationToken)
        {
            var cityRepo = _unitOfWork.GetRepository<City, Guid>();
            var city = await cityRepo.GetByIdAsync(request.Id, cancellationToken);
            if (city == null || city.IsDeleted)
            {
                return Result<CityDto>.NotFound(LocalizationKeys.City.NotFound);
            }

            var targetCountryId = request.CountryId ?? city.CountryId;
            if (request.CountryId.HasValue)
            {
                var countryRepo = _unitOfWork.GetRepository<Country, Guid>();
                var country = await countryRepo.GetByIdAsync(request.CountryId.Value, cancellationToken);
                if (country == null || country.IsDeleted)
                {
                    return Result<CityDto>.NotFound(LocalizationKeys.Country.NotFound);
                }
            }

            var targetNameEn = !string.IsNullOrWhiteSpace(request.NameEn) ? request.NameEn.Trim() : city.NameEn;
            var targetNameAr = !string.IsNullOrWhiteSpace(request.NameAr) ? request.NameAr.Trim() : city.NameAr;

            var exists = await cityRepo.ExistsAsync(
                c => !c.IsDeleted && c.Id != city.Id && c.CountryId == targetCountryId &&
                     (c.NameEn.ToLower() == targetNameEn.ToLower() || c.NameAr == targetNameAr),
                cancellationToken);

            if (exists)
            {
                return Result<CityDto>.Conflict(LocalizationKeys.City.AlreadyExists);
            }

            var currentUserId = _currentUserService.UserId != Guid.Empty
                ? _currentUserService.UserId.ToString()
                : "System";

            city.Update(request.CountryId, request.NameEn, request.NameAr, currentUserId);
            cityRepo.Update(city);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var updatedCity = await cityRepo
                .GetAllWithIncluding(c => c.Id == city.Id, c => c.Country)
                .FirstOrDefaultAsync(cancellationToken);

            var dto = new CityDto
            {
                Id = city.Id,
                CountryId = city.CountryId,
                CountryNameEn = updatedCity?.Country?.NameEn,
                CountryNameAr = updatedCity?.Country?.NameAr,
                NameEn = city.NameEn,
                NameAr = city.NameAr,
                IsActive = city.IsActive,
                CreatedAt = city.CreatedAt
            };

            return Result<CityDto>.Success(dto, LocalizationKeys.City.Updated);
        }
    }
}
