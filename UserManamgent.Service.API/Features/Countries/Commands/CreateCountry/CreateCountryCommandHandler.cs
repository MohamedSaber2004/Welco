using MediatR;
using Welco.Shared.Common.DTOs.UserManagement;
using Welco.Shared.Common.Interfaces;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Domain.Models;
using Welco.Shared.Localization;
using Welco.Shared.Results;

namespace UserManamgent.Service.API.Features.Countries.Commands.CreateCountry
{
    public class CreateCountryCommandHandler : IRequestHandler<CreateCountryCommand, Result<CountryDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public CreateCountryCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<Result<CountryDto>> Handle(CreateCountryCommand request, CancellationToken cancellationToken)
        {
            var countryRepo = _unitOfWork.GetRepository<Country, Guid>();
            var exists = await countryRepo.ExistsAsync(
                c => (!c.IsDeleted) && (c.NameEn.ToLower() == request.NameEn.Trim().ToLower() || c.NameAr == request.NameAr.Trim()),
                cancellationToken);

            if (exists)
            {
                return Result<CountryDto>.Conflict(LocalizationKeys.Country.AlreadyExists);
            }

            var currentUserId = _currentUserService.UserId != Guid.Empty
                ? _currentUserService.UserId.ToString()
                : "System";

            var country = Country.Create(
                request.NameEn.Trim(),
                request.NameAr.Trim(),
                request.Code?.Trim(),
                currentUserId);

            await countryRepo.AddAsync(country, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var dto = new CountryDto
            {
                Id = country.Id,
                NameEn = country.NameEn,
                NameAr = country.NameAr,
                Code = country.Code,
                IsActive = country.IsActive,
                CreatedAt = country.CreatedAt
            };

            return Result<CountryDto>.Created(dto, LocalizationKeys.Country.Created);
        }
    }
}
