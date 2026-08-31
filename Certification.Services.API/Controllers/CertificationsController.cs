using Certification.Services.API.CertificationRoutes;
using Certification.Services.API.Features.Certifications.Commands.CreateCertification;
using Certification.Services.API.Features.Certifications.Commands.DeleteCertification;
using Certification.Services.API.Features.Certifications.Commands.UpdateCertification;
using Certification.Services.API.Features.Certifications.Queries.GetCertificationById;
using Certification.Services.API.Features.Certifications.Queries.GetCertifications;
using Certification.Services.API.Features.Certifications.Queries.ShowCertification;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Welco.Shared.Common.Attributes;
using Welco.Shared.Controllers;

namespace Certification.Services.API.Controllers
{
    [RoleAuthorize]
    [Route(CertificationApiRoutes.Certifications.Base)]
    public class CertificationsController : AppControllerBase
    {
        public CertificationsController(IMediator mediator) : base(mediator)
        {
        }

        /// <summary>
        /// Get All Certifications.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet]
        [Route(CertificationApiRoutes.Certifications.GetAll)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll([FromQuery] GetCertificationsQuery query, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(query, cancellationToken);
            return ToActionResult(result);
        }

        /// <summary>
        /// Get Certification By Id.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet]
        [Route(CertificationApiRoutes.Certifications.GetById)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetCertificationByIdQuery { Id = id }, cancellationToken);
            return ToActionResult(result);
        }

        /// <summary>
        /// Show Certification.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet]
        [Route(CertificationApiRoutes.Certifications.Show)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Show([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new ShowCertificationQuery { Id = id }, cancellationToken);
            return ToActionResult(result);
        }

        /// <summary>
        /// Create Certification.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost]
        [Route(CertificationApiRoutes.Certifications.Create)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Create([FromBody] CreateCertificationCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return ToActionResult(result);
        }

        /// <summary>
        /// Update Certification.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="command"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPut]
        [Route(CertificationApiRoutes.Certifications.Update)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateCertificationCommand command, CancellationToken cancellationToken)
        {
            command.Id = id;
            var result = await _mediator.Send(command, cancellationToken);
            return ToActionResult(result);
        }

        /// <summary>
        /// Delete Certification.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route(CertificationApiRoutes.Certifications.Delete)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new DeleteCertificationCommand { Id = id }, cancellationToken);
            return ToActionResult(result);
        }
    }
}
