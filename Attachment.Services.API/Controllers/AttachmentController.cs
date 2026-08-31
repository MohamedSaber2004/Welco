using Attachment.Services.API.AttachmentRoutes;
using Attachment.Services.API.Features.Attachments.Commands.DownloadFile;
using Attachment.Services.API.Features.Attachments.Commands.UpdateFile;
using Attachment.Services.API.Features.Attachments.Commands.UploadFile;
using Attachment.Services.API.Features.Attachments.Commands.UploadMultiple;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Welco.Shared.Common.Attributes;
using Welco.Shared.Controllers;
using Welco.Shared.Enums;
using Welco.Shared.Localization;

namespace Attachment.Services.API.Controllers
{
    [RoleAuthorize]
    [Route(AttachmentApiRoutes.Base)]
    public class AttachmentController : AppControllerBase
    {
        public AttachmentController(IMediator mediator) : base(mediator)
        {
        }

        /// <summary>
        /// Upload a file.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost]
        [Route(AttachmentApiRoutes.Attachments.Upload)]
        [RoleAuthorize(UserType.OrganizationUser, UserType.WelcoStaff, UserType.Admin)]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Upload([FromForm] UploadFileCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return CreatedResult(result, LocalizationKeys.AttachmentMessages.FileUploaded);
        }

        /// <summary>
        /// Upload multiple files.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost]
        [Route(AttachmentApiRoutes.Attachments.UploadMultiple)]
        [RoleAuthorize(UserType.OrganizationUser, UserType.WelcoStaff, UserType.Admin)]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UploadMultiple([FromForm] UploadMultipleFilesCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return CreatedResult(result, LocalizationKeys.AttachmentMessages.FileUploaded);
        }

        /// <summary>
        /// Update a file by its name.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="command"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPut]
        [Route(AttachmentApiRoutes.Attachments.Update)]
        [RoleAuthorize(UserType.OrganizationUser, UserType.WelcoStaff, UserType.Admin)]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update([FromRoute] string name, [FromForm] UpdateFileCommand command, CancellationToken cancellationToken)
        {
            command.OldFileName = name;
            var result = await _mediator.Send(command, cancellationToken);
            return Success(result, LocalizationKeys.AttachmentMessages.FileUploaded);
        }

        /// <summary>
        /// Download a file by its name.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet]
        [Route(AttachmentApiRoutes.Attachments.Download)]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Download([FromQuery] DownloadFileCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return Success(result);
        }
    }
}
