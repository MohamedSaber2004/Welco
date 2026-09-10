using Auth.Services.API.AuthRoutes;
using Auth.Services.API.Features.Auth.Commands.ForgotPassword;
using Auth.Services.API.Features.Auth.Commands.Login;
using Auth.Services.API.Features.Auth.Commands.Logout;
using Auth.Services.API.Features.Auth.Commands.RefreshToken;
using Auth.Services.API.Features.Auth.Commands.Register;
using Auth.Services.API.Features.Auth.Commands.ResetPassword;
using Auth.Services.API.Features.Auth.Commands.UpdateProfile;
using Auth.Services.API.Features.Auth.Commands.VerifyEmailOtp;
using Auth.Services.API.Features.Auth.Commands.VerifyPasswordOtp;
using Auth.Services.API.Features.Auth.Queries.GetUserProfile;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Welco.Shared.Common.Attributes;
using Welco.Shared.Controllers;

namespace Auth.Services.API.Controllers
{
        [Route(AuthApiRoutes.Base)]
    public class AuthController : AppControllerBase
    {
        public AuthController(IMediator mediator) : base(mediator)
        {
        }

                [HttpPost]
        [Route(AuthApiRoutes.Authentication.Register)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return ToActionResult(result);
        }

                [HttpPost]
        [Route(AuthApiRoutes.Authentication.Login)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return ToActionResult(result);
        }

                [HttpPost]
        [Route(AuthApiRoutes.Authentication.VerifyEmailOtp)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> VerifyEmailOtp([FromBody] VerifyEmailOtpCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return ToActionResult(result);
        }

                [HttpPost]
        [Route(AuthApiRoutes.Authentication.ForgotPassword)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return ToActionResult(result);
        }

                [HttpPost]
        [Route(AuthApiRoutes.Authentication.VerifyPasswordOtp)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> VerifyPasswordOtp([FromBody] VerifyPasswordOtpCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return ToActionResult(result);
        }

                [HttpPost]
        [Route(AuthApiRoutes.Authentication.ResetPassword)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return ToActionResult(result);
        }

                [HttpPost]
        [Route(AuthApiRoutes.Authentication.RefreshToken)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return ToActionResult(result);
        }

                [HttpPost]
        [Route(AuthApiRoutes.Authentication.Logout)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Logout([FromBody] LogoutCommand? command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command ?? new LogoutCommand(), cancellationToken);
            return ToActionResult(result);
        }

                [HttpGet]
        [RoleAuthorize]
        [Route(AuthApiRoutes.Authentication.Profile)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProfile(CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetUserProfileQuery(), cancellationToken);
            return ToActionResult(result);
        }

                [HttpPut]
        [RoleAuthorize]
        [Route(AuthApiRoutes.Authentication.Profile)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return ToActionResult(result);
        }
    }
}
