using MediatR;
using Welco.Shared.Common.DTOs.Certifications;
using Welco.Shared.Results;

namespace Certification.Services.API.Features.Certifications.Queries.ShowCertification
{
    public class ShowCertificationQuery : IRequest<Result<CertificationDto>>
    {
        public Guid Id { get; set; }
    }
}
