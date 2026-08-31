using Content.Services.API.ContentRoutes;
using Content.Services.API.Features.Documents.Commands.CreateDocument;
using Content.Services.API.Features.Documents.Commands.DeleteDocument;
using Content.Services.API.Features.Documents.Queries.GetDocumentById;
using Content.Services.API.Features.Documents.Queries.GetDocuments;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Welco.Shared.Common.Attributes;
using Welco.Shared.Controllers;
using Welco.Shared.Enums;

namespace Content.Services.API.Controllers
{
    [RoleAuthorize]
    [Route(ContentApiRoutes.Documents.Base)]
    public class DocumentsController : AppControllerBase
    {
        public DocumentsController(IMediator mediator) : base(mediator) { }

        /// <summary>
        /// Get All Documents
        /// </summary>
        [HttpGet]
        [Route(ContentApiRoutes.Documents.GetAll)]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll([FromQuery] GetDocumentsQuery q, CancellationToken ct) => ToActionResult(await _mediator.Send(q, ct));

        /// <summary>
        /// Get Document By Id
        /// </summary>
        [HttpGet]
        [Route(ContentApiRoutes.Documents.GetById)]
        [AllowAnonymous]
        public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken ct) => ToActionResult(await _mediator.Send(new GetDocumentByIdQuery { Id = id }, ct));

        /// <summary>
        /// Create Document
        /// </summary>
        [HttpPost]
        [Route(ContentApiRoutes.Documents.Create)]
        [RoleAuthorize(UserType.Admin, UserType.WelcoStaff)]
        public async Task<IActionResult> Create([FromBody] CreateDocumentCommand cmd, CancellationToken ct) => ToActionResult(await _mediator.Send(cmd, ct));

        /// <summary>
        /// Delete Document
        /// </summary>
        [HttpDelete]
        [Route(ContentApiRoutes.Documents.Delete)]
        [RoleAuthorize(UserType.Admin, UserType.WelcoStaff)]
        public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken ct) => ToActionResult(await _mediator.Send(new DeleteDocumentCommand { Id = id }, ct));
    }
}
