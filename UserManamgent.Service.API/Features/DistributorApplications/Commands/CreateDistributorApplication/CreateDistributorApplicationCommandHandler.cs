using MediatR;
using Welco.Shared.Common.DTOs.UserManagement;
using Welco.Shared.Common.Interfaces;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Domain.Models;
using Welco.Shared.Localization;
using Welco.Shared.Results;

namespace UserManamgent.Service.API.Features.DistributorApplications.Commands.CreateDistributorApplication
{
    public class CreateDistributorApplicationCommandHandler : IRequestHandler<CreateDistributorApplicationCommand, Result<DistributorApplicationDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public CreateDistributorApplicationCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<Result<DistributorApplicationDto>> Handle(CreateDistributorApplicationCommand request, CancellationToken cancellationToken)
        {
            // OrganizationUser or Admin allowed â€” WelcoStaff blocked via RoleAuthorize, but double-check
            // Validate CountryId exists in DB (country found in database)
            var countryRepo = _unitOfWork.GetRepository<Country, Guid>();
            var country = await countryRepo.GetByIdAsync(request.CountryId, cancellationToken);
            if (country == null || country.IsDeleted)
            {
                return Result<DistributorApplicationDto>.NotFound(LocalizationKeys.Country.NotFound);
            }

            var currentUserId = _currentUserService.UserId != Guid.Empty
                ? _currentUserService.UserId.ToString()
                : "System";

            var app = new DistributorApplication
            {
                Id = Guid.NewGuid(),
                CompanyName = request.CompanyName.Trim(),
                CountryId = request.CountryId,
                SalesVolumeBand = string.IsNullOrWhiteSpace(request.SalesVolumeBand) ? "Not specified" : request.SalesVolumeBand.Trim(),
                CategoryInterest = string.IsNullOrWhiteSpace(request.CategoryInterest) ? null : request.CategoryInterest.Trim(),
                Website = string.IsNullOrWhiteSpace(request.Website) ? null : request.Website.Trim(),
                ContactPerson = request.ContactPerson.Trim(),
                ContactEmail = request.Email.Trim(),
                Phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim(),
                Status = DistributorApplicationStatus.Pending,
            };
            app.MarkAsCreated(currentUserId);

            var repo = _unitOfWork.GetRepository<DistributorApplication, Guid>();
            await repo.AddAsync(app, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var dto = new DistributorApplicationDto
            {
                Id = app.Id,
                CompanyName = app.CompanyName,
                CountryId = app.CountryId,
                CountryNameEn = country.NameEn,
                SalesVolumeBand = app.SalesVolumeBand,
                CategoryInterest = app.CategoryInterest,
                Website = app.Website,
                ContactPerson = app.ContactPerson,
                ContactEmail = app.ContactEmail,
                Phone = app.Phone,
                Status = app.Status.ToString(),
                CreatedAt = app.CreatedAt
            };

            return Result<DistributorApplicationDto>.Created(dto, "Distributor application submitted");
        }
    }
}
