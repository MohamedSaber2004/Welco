using MediatR;
using Welco.Shared.Results;

namespace Certification.Services.API.Features.Certifications.Commands.DeleteCertification
{
    public class DeleteCertificationCommand : IRequest<Result<string>>
    {
        public Guid Id { get; set; }
    }
}
