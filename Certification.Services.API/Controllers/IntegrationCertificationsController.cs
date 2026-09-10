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

namespace Certification.Services.API.Controllers
{
        [ServiceAuth]
    [ApiController]
    [IntegrationRouteName("Certifications")]
    [Route("api/integration/certifications")]
    public class IntegrationCertificationsController : AppControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public IntegrationCertificationsController(IMediator mediator, IUnitOfWork unitOfWork) : base(mediator)
        {
            _unitOfWork = unitOfWork;
        }

                [HttpGet("")]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            var repo = _unitOfWork.GetRepository<Welco.Shared.Domain.Models.Certification, Guid>();
            var certs = await repo.GetAll(c => !c.IsDeleted)
                .OrderBy(c => c.Title)
                .Select(c => new ExternalCertificationDto
                {
                    Id = c.Id,
                    CertificateNumber = c.CertificateNumber,
                    Title = c.Title,
                    IssuedTo = c.IssuedTo,
                    Issuer = c.Issuer,
                    IssueDate = c.IssueDate,
                    ExpiryDate = c.ExpiryDate,
                    Description = c.Description,
                    CertificationImageName = c.CertificationImageName
                })
                .AsNoTracking()
                .ToListAsync(ct);

            return ToActionResult(Result<List<ExternalCertificationDto>>.Success(certs));
        }

                [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken ct)
        {
            var repo = _unitOfWork.GetRepository<Welco.Shared.Domain.Models.Certification, Guid>();
            var cert = await repo.GetAll(c => !c.IsDeleted && c.Id == id)
                .Select(c => new ExternalCertificationDto
                {
                    Id = c.Id,
                    CertificateNumber = c.CertificateNumber,
                    Title = c.Title,
                    IssuedTo = c.IssuedTo,
                    Issuer = c.Issuer,
                    IssueDate = c.IssueDate,
                    ExpiryDate = c.ExpiryDate,
                    Description = c.Description,
                    CertificationImageName = c.CertificationImageName
                })
                .AsNoTracking()
                .FirstOrDefaultAsync(ct);

            if (cert == null)
                return ToActionResult(Result<ExternalCertificationDto>.NotFound("Certification not found."));

            return ToActionResult(Result<ExternalCertificationDto>.Success(cert));
        }
    }
}
