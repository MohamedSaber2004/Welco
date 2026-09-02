using MediatR;
using Microsoft.EntityFrameworkCore;
using Welco.Shared.Common.DTOs.UserManagement;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Domain.Models;
using Welco.Shared.Localization;
using Welco.Shared.Results;

namespace UserManamgent.Service.API.Features.Addresses.Queries.GetAddressById
{
    public class GetAddressByIdQueryHandler : IRequestHandler<GetAddressByIdQuery, Result<UserAddressDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetAddressByIdQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<UserAddressDto>> Handle(GetAddressByIdQuery request, CancellationToken cancellationToken)
        {
            var addressRepo = _unitOfWork.GetRepository<UserAddress, Guid>();
            var address = await addressRepo
                .GetAllWithIncluding(a => a.Id == request.Id && !a.IsDeleted, a => a.Country, a => a.City, a => a.Zone)
                .FirstOrDefaultAsync(cancellationToken);

            if (address == null)
            {
                return Result<UserAddressDto>.NotFound(LocalizationKeys.UserAddress.AddressNotFound);
            }

            var addressDto = new UserAddressDto
            {
                Id = address.Id,
                UserId = address.UserId,
                CountryId = address.CountryId,
                CountryNameEn = address.Country?.NameEn,
                CountryNameAr = address.Country?.NameAr,
                CountryCode = address.Country?.Code,
                CountryPhoneCode = address.Country?.PhoneCode,
                CityId = address.CityId,
                CityNameEn = address.City?.NameEn,
                CityNameAr = address.City?.NameAr,
                ZoneId = address.ZoneId,
                ZoneNameEn = address.Zone?.NameEn,
                ZoneNameAr = address.Zone?.NameAr,
                Street = address.Street,
                Building = address.Building,
                Floor = address.Floor,
                Apartment = address.Apartment,
                IsDefault = address.IsDefault,
                CreatedAt = address.CreatedAt,
                UpdatedAt = address.UpdatedAt
            };

            return Result<UserAddressDto>.Success(addressDto, LocalizationKeys.UserAddress.AddressFetched);
        }
    }
}
