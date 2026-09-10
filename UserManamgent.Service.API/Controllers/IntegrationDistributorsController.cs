using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Welco.Shared.Common.Attributes;
using Welco.Shared.Common.DTOs.Integration;
using Welco.Shared.Common.Extensions;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Controllers;
using Welco.Shared.Domain.Models;
using Welco.Shared.Results;

namespace UserManamgent.Service.API.Controllers
{
        [ServiceAuth]
    [ApiController]
    [IntegrationRouteName("Distributors")]
    [Route("api/integration/distributors")]
    public class IntegrationDistributorsController : AppControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public IntegrationDistributorsController(IMediator mediator, IUnitOfWork unitOfWork) : base(mediator)
        {
            _unitOfWork = unitOfWork;
        }

                [HttpPost("apply")]
        public async Task<IActionResult> Apply([FromBody] ApplyDistributorRequest request, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(request.CompanyName) || string.IsNullOrWhiteSpace(request.Email))
                return ToActionResult(Result<DistributorApplicationDto>.BadRequest("Company name and email are required."));

            var countryRepo = _unitOfWork.GetRepository<Country, Guid>();
            var country = await countryRepo.GetByIdAsync(request.CountryId, ct);
            if (country == null || country.IsDeleted)
            {
                
                country = await countryRepo.GetAll(c => !c.IsDeleted).FirstOrDefaultAsync(ct);
                if (country == null)
                    return ToActionResult(Result<DistributorApplicationDto>.BadRequest("No valid country configured."));
            }

            var app = new DistributorApplication
            {
                Id = Guid.NewGuid(),
                CompanyName = request.CompanyName.Trim(),
                CountryId = country.Id,
                SalesVolumeBand = string.IsNullOrWhiteSpace(request.SalesVolumeBand) ? "Not specified" : request.SalesVolumeBand.Trim(),
                CategoryInterest = string.IsNullOrWhiteSpace(request.CategoryInterest) ? null : request.CategoryInterest.Trim(),
                Website = string.IsNullOrWhiteSpace(request.Website) ? null : request.Website.Trim(),
                ContactPerson = request.ContactPerson.Trim(),
                ContactEmail = request.Email.Trim(),
                Phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim(),
                Status = DistributorApplicationStatus.Pending
            };
            app.MarkAsCreated($"integration-{request.SourceMarket.ToLowerInvariant()}");

            var repo = _unitOfWork.GetRepository<DistributorApplication, Guid>();
            await repo.AddAsync(app, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            var dto = new DistributorApplicationDto
            {
                Id = app.Id,
                CompanyName = app.CompanyName,
                ContactPerson = app.ContactPerson,
                Email = app.ContactEmail,
                Phone = app.Phone,
                CountryId = app.CountryId,
                CountryName = country.NameEn,
                SalesVolumeBand = app.SalesVolumeBand,
                CategoryInterest = app.CategoryInterest,
                Website = app.Website,
                Status = app.Status.ToString(),
                CreatedAt = app.CreatedAt
            };

            return ToActionResult(Result<DistributorApplicationDto>.Created(dto));
        }

                [HttpGet("")]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            var repo = _unitOfWork.GetRepository<DistributorApplication, Guid>();
            var apps = await repo.GetAll(a => !a.IsDeleted)
                .Include(a => a.Country)
                .OrderByDescending(a => a.CreatedAt)
                .Select(a => new DistributorApplicationDto
                {
                    Id = a.Id,
                    CompanyName = a.CompanyName,
                    ContactPerson = a.ContactPerson,
                    Email = a.ContactEmail,
                    Phone = a.Phone,
                    CountryId = a.CountryId,
                    CountryName = a.Country != null ? a.Country.NameEn : null,
                    SalesVolumeBand = a.SalesVolumeBand,
                    CategoryInterest = a.CategoryInterest,
                    Website = a.Website,
                    Status = a.Status.ToString(),
                    CreatedAt = a.CreatedAt
                })
                .AsNoTracking()
                .ToListAsync(ct);

            return ToActionResult(Result<List<DistributorApplicationDto>>.Success(apps));
        }

                [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken ct)
        {
            var repo = _unitOfWork.GetRepository<DistributorApplication, Guid>();
            var app = await repo.GetAll(a => !a.IsDeleted && a.Id == id)
                .Include(a => a.Country)
                .Select(a => new DistributorApplicationDto
                {
                    Id = a.Id,
                    CompanyName = a.CompanyName,
                    ContactPerson = a.ContactPerson,
                    Email = a.ContactEmail,
                    Phone = a.Phone,
                    CountryId = a.CountryId,
                    CountryName = a.Country != null ? a.Country.NameEn : null,
                    SalesVolumeBand = a.SalesVolumeBand,
                    CategoryInterest = a.CategoryInterest,
                    Website = a.Website,
                    Status = a.Status.ToString(),
                    CreatedAt = a.CreatedAt
                })
                .AsNoTracking()
                .FirstOrDefaultAsync(ct);

            if (app == null)
                return ToActionResult(Result<DistributorApplicationDto>.NotFound("Distributor application not found."));

            return ToActionResult(Result<DistributorApplicationDto>.Success(app));
        }

                [HttpPut("{id:guid}/approve")]
        public async Task<IActionResult> Approve([FromRoute] Guid id, CancellationToken ct)
        {
            var repo = _unitOfWork.GetRepository<DistributorApplication, Guid>();
            var app = await repo.GetByIdAsync(id, ct);
            if (app == null || app.IsDeleted)
                return ToActionResult(Result<bool>.NotFound("Distributor application not found."));

            app.Status = DistributorApplicationStatus.Approved;
            app.MarkAsUpdated("integration-service");

var companyRepo = _unitOfWork.GetRepository<Company, Guid>();
            var existingCompany = await companyRepo.GetAll(c => !c.IsDeleted && c.Name == app.CompanyName).FirstOrDefaultAsync(ct);
            if (existingCompany == null)
            {
                var newCompany = Company.Create(
                    name: app.CompanyName,
                    type: Welco.Shared.Enums.CompanyType.Distributor,
                    countryId: app.CountryId,
                    status: Welco.Shared.Enums.CompanyStatus.Approved,
                    accountManagerId: null,
                    createdBy: "integration-service",
                    email: app.ContactEmail);
                newCompany.IsProvider = true;
                await companyRepo.AddAsync(newCompany, ct);
            }
            else
            {
                existingCompany.IsProvider = true;
                existingCompany.Status = Welco.Shared.Enums.CompanyStatus.Approved;
                existingCompany.MarkAsUpdated("integration-service");
            }

            await _unitOfWork.SaveChangesAsync(ct);
            return ToActionResult(Result<bool>.Success(true));
        }

                [HttpPut("{id:guid}/reject")]
        public async Task<IActionResult> Reject([FromRoute] Guid id, [FromBody] UpdateDistributorStatusRequest request, CancellationToken ct)
        {
            var repo = _unitOfWork.GetRepository<DistributorApplication, Guid>();
            var app = await repo.GetByIdAsync(id, ct);
            if (app == null || app.IsDeleted)
                return ToActionResult(Result<bool>.NotFound("Distributor application not found."));

            app.Status = DistributorApplicationStatus.Rejected;
            app.MarkAsUpdated("integration-service");
            await _unitOfWork.SaveChangesAsync(ct);

            return ToActionResult(Result<bool>.Success(true));
        }
    }
}
