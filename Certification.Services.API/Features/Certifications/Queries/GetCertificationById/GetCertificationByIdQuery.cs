using MediatR;
using Welco.Shared.Common.DTOs.Certifications;
using Welco.Shared.Results;

namespace Certification.Services.API.Features.Certifications.Queries.GetCertificationById
{
    public class GetCertificationByIdQuery : IRequest<Result<CertificationDto>>
    {
        public Guid Id { get; set; }
    }
}
