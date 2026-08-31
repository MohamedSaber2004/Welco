using MediatR;
using Welco.Shared.Common.DTOs.Certifications;
using Welco.Shared.Results;

namespace Certification.Services.API.Features.Certifications.Commands.CreateCertification
{
    public class CreateCertificationCommand : IRequest<Result<CertificationDto>>
    {
        public string CertificateNumber { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string IssuedTo { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public DateTime IssueDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string? Description { get; set; }
        public string? CertificationImageName { get; set; }
    }
}
