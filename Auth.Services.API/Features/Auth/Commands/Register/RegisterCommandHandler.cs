using System.Security.Cryptography;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Welco.Shared.Common.DTOs.Auth.Responses;
using Welco.Shared.Common.Interfaces;
using Welco.Shared.Common.Options;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Domain.Models;
using Welco.Shared.Enums;
using Welco.Shared.Localization;
using Welco.Shared.Results;

namespace Auth.Services.API.Features.Auth.Commands.Register
{
    public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<string>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly EmailSettings _emailSettings;

        public RegisterCommandHandler(
            UserManager<ApplicationUser> userManager,
            IEmailService emailService,
            IOptions<EmailSettings> emailSettings)
        {
            _userManager = userManager;
            _emailService = emailService;
            _emailSettings = emailSettings.Value;
        }

        public async Task<Result<string>> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            var expiryMinutes = _emailSettings.VerificationCodeExpiryMinutes > 0 ? _emailSettings.VerificationCodeExpiryMinutes : 10;
            var emailOtp = RandomNumberGenerator.GetInt32(100000, 999999).ToString();

            var user = new ApplicationUser
            {
                FullName = request.FullName,
                Email = request.Email,
                UserName = request.Email,
                PhoneNumber = request.PhoneNumber,
                UserType = request.UserType,
                Language = request.Language,
                IsActive = false,
                EmailConfirmed = false,
                EmailConfirmationOtp = emailOtp,
                EmailConfirmationOtpExpiry = DateTime.UtcNow.AddMinutes(expiryMinutes)
            };

            var createResult = await _userManager.CreateAsync(user, request.Password);
            if (!createResult.Succeeded)
            {
                var errors = createResult.Errors.Select(e => e.Description).ToList();
                return Result<string>.BadRequest(
                    errors.FirstOrDefault() ?? LocalizationKeys.ExceptionMessages.BadRequest,
                    errors);
            }

            await _userManager.AddToRoleAsync(user, request.UserType.ToString());

            try
            {
                await _emailService.SendVerificationEmailAsync(user.Email!, emailOtp, user.Language.ToString().ToLower(), cancellationToken);
            }
            catch (Exception)
            {
                // Email sending failed or is not configured; user account & OTP were safely created
            }

            return Result<string>.Success(user.Email!, LocalizationKeys.Auth.RegisterSuccess);
        }
    }
}
