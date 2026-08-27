using Auth.Services.API.AuthRoutes;
using Auth.Services.API.Features.Auth.Commands.ForgotPassword;
using Auth.Services.API.Features.Auth.Commands.Login;
using Auth.Services.API.Features.Auth.Commands.RefreshToken;
using Auth.Services.API.Features.Auth.Commands.Register;
using Auth.Services.API.Features.Auth.Commands.ResetPassword;
using Auth.Services.API.Features.Auth.Commands.VerifyEmailOtp;
using Auth.Services.API.Features.Auth.Commands.VerifyPasswordOtp;
using Auth.Services.API.Features.Auth.Queries.GetUserProfile;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Welco.Shared.Common.Attributes;
using Welco.Shared.Controllers;

namespace Auth.Services.API.Controllers
{
    /// <summary>
    /// Authentication and User Management Controller
    /// </summary>
    [Route(AuthApiRoutes.Base)]
    public class AuthController : AppControllerBase
    {
        public AuthController(IMediator mediator) : base(mediator)
        {
        }

        /// <summary>
        /// Register a new user with email and password.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost]
        [Route(AuthApiRoutes.Authentication.Register)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return ToActionResult(result);
        }

        /// <summary>
        /// Login a user with email and password.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Verify the OTP sent to the user's email during registration.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Initiate the forgot password process by sending an OTP to the user's email.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Verify the OTP sent to the user's email for password reset.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Reset the password for a user after verifying the OTP.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Refresh the access token using a valid refresh token.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost]
        [Route(AuthApiRoutes.Authentication.RefreshToken)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return ToActionResult(result);
        }

        /// <summary>
        /// Get the profile of the currently authenticated user.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Diagnostic endpoint to test email configuration and server outbound connectivity.
        /// </summary>
        [HttpGet]
        [Route(AuthApiRoutes.Authentication.TestEmail)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> TestEmail(
            [FromQuery] string toEmail,
            [FromServices] Microsoft.Extensions.Options.IOptions<Welco.Shared.Common.Options.EmailSettings> emailOptions,
            CancellationToken cancellationToken)
        {
            var settings = emailOptions.Value;
            var diagnostics = new Dictionary<string, object?>();
            diagnostics["Configured_Host"] = settings.Host;
            diagnostics["Configured_Port"] = settings.Port;
            diagnostics["Configured_Username"] = settings.Username;
            diagnostics["Configured_Email"] = settings.Email;
            diagnostics["Configured_Password_Present"] = !string.IsNullOrEmpty(settings.Password);

            if (string.IsNullOrWhiteSpace(toEmail))
            {
                return BadRequest(new { isSuccess = false, message = "Query parameter 'toEmail' is required.", data = diagnostics });
            }

            // 1. DNS Resolution Test
            try
            {
                var addresses = await System.Net.Dns.GetHostAddressesAsync(settings.Host);
                diagnostics["DNS_Resolution"] = $"Success: {string.Join(", ", addresses.Select(a => a.ToString()))}";
            }
            catch (Exception ex)
            {
                diagnostics["DNS_Resolution"] = $"Failed: {ex.Message}";
            }

            // 2. TCP Port 587 Test
            try
            {
                using var tcp587 = new System.Net.Sockets.TcpClient();
                var connectTask = tcp587.ConnectAsync(settings.Host, 587);
                if (await Task.WhenAny(connectTask, Task.Delay(5000)) == connectTask && tcp587.Connected)
                {
                    diagnostics["TCP_Port_587"] = "Open & Reachable";
                }
                else
                {
                    diagnostics["TCP_Port_587"] = "Blocked / Timed Out (Hosting firewall blocks port 587)";
                }
            }
            catch (Exception ex)
            {
                diagnostics["TCP_Port_587"] = $"Failed: {ex.Message}";
            }

            // 3. TCP Port 465 Test
            try
            {
                using var tcp465 = new System.Net.Sockets.TcpClient();
                var connectTask = tcp465.ConnectAsync(settings.Host, 465);
                if (await Task.WhenAny(connectTask, Task.Delay(5000)) == connectTask && tcp465.Connected)
                {
                    diagnostics["TCP_Port_465"] = "Open & Reachable";
                }
                else
                {
                    diagnostics["TCP_Port_465"] = "Blocked / Timed Out (Hosting firewall blocks port 465)";
                }
            }
            catch (Exception ex)
            {
                diagnostics["TCP_Port_465"] = $"Failed: {ex.Message}";
            }

            // 4. MailKit Live Send Test
            try
            {
                var fromName = string.IsNullOrWhiteSpace(settings.Name) ? "Welco Team" : settings.Name;
                var fromEmail = !string.IsNullOrWhiteSpace(settings.Email) ? settings.Email : settings.Username;

                var email = new MimeKit.MimeMessage();
                email.From.Add(new MimeKit.MailboxAddress(fromName, fromEmail));
                email.To.Add(MimeKit.MailboxAddress.Parse(toEmail));
                email.Subject = "Welco Diagnostic Test Email";
                email.Body = new MimeKit.TextPart(MimeKit.Text.TextFormat.Plain)
                {
                    Text = $"This is a test email sent from Welco Server diagnostic endpoint at {DateTime.UtcNow} UTC."
                };

                using var smtp = new MailKit.Net.Smtp.SmtpClient();
                smtp.Timeout = 10000;
                smtp.ServerCertificateValidationCallback = (s, c, h, e) => true;

                var socketOptions = settings.Port switch
                {
                    465 => MailKit.Security.SecureSocketOptions.SslOnConnect,
                    587 => MailKit.Security.SecureSocketOptions.StartTls,
                    25 => MailKit.Security.SecureSocketOptions.None,
                    _ => MailKit.Security.SecureSocketOptions.Auto
                };

                try
                {
                    await smtp.ConnectAsync(settings.Host, settings.Port, socketOptions, cancellationToken);
                    diagnostics["SMTP_Connect_Result"] = $"Connected to {settings.Host}:{settings.Port}";
                }
                catch (Exception connEx) when (settings.Port == 587)
                {
                    diagnostics["SMTP_Port_587_Fallback"] = $"Failed on 587 ({connEx.Message}). Trying 465 SSL...";
                    await smtp.ConnectAsync(settings.Host, 465, MailKit.Security.SecureSocketOptions.SslOnConnect, cancellationToken);
                    diagnostics["SMTP_Connect_Result"] = $"Connected to {settings.Host}:465 (SSL)";
                }

                if (!string.IsNullOrWhiteSpace(settings.Username) && !string.IsNullOrWhiteSpace(settings.Password))
                {
                    await smtp.AuthenticateAsync(settings.Username, settings.Password, cancellationToken);
                    diagnostics["SMTP_Auth_Result"] = "Authenticated successfully";
                }

                await smtp.SendAsync(email, cancellationToken);
                await smtp.DisconnectAsync(true, cancellationToken);

                diagnostics["MailKit_Send_Result"] = "SUCCESS: Email sent and acknowledged by SMTP server!";
                return Ok(new { isSuccess = true, message = "Email sent successfully!", data = diagnostics });
            }
            catch (Exception ex)
            {
                diagnostics["MailKit_Send_Result"] = $"FAILED: {ex.GetType().Name}: {ex.Message}";
                diagnostics["Error_Stack"] = ex.ToString();
                return StatusCode(StatusCodes.Status500InternalServerError, new { isSuccess = false, message = "Email sending failed", data = diagnostics });
            }
        }
    }
}
