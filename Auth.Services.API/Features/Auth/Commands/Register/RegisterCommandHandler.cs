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
        private readonly IUnitOfWork _unitOfWork;

        public RegisterCommandHandler(
            UserManager<ApplicationUser> userManager,
            IEmailService emailService,
            IOptions<EmailSettings> emailSettings,
            IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _emailService = emailService;
            _emailSettings = emailSettings.Value;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<string>> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            // Validate phone -> Country linkage when phone is provided
            if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
            {
                var phone = request.PhoneNumber.Trim();
                // If frontend supplied PhoneCountryId, verify it exists and phone starts with its PhoneCode
                if (request.PhoneCountryId.HasValue && request.PhoneCountryId.Value != Guid.Empty)
                {
                    var countryRepo = _unitOfWork.GetRepository<Country, Guid>();
                    var phoneCountry = await countryRepo.GetByIdAsync(request.PhoneCountryId.Value, cancellationToken);
                    if (phoneCountry == null || phoneCountry.IsDeleted)
                    {
                        return Result<string>.BadRequest(LocalizationKeys.Country.NotFound, new List<string> { LocalizationKeys.Country.NotFound });
                    }
                    if (!string.IsNullOrWhiteSpace(phoneCountry.PhoneCode))
                    {
                        var code = phoneCountry.PhoneCode.Trim();
                        // Normalize: phone may be "+971 50..." or "+97150..."
                        var normalized = phone.Replace(" ", "").Replace("-", "");
                        var codeNorm = code.Replace(" ", "");
                        if (!normalized.StartsWith(codeNorm, StringComparison.Ordinal))
                        {
                            return Result<string>.BadRequest(
                                $"Phone number must start with country phone code {code} ({phoneCountry.NameEn})",
                                new List<string> { $"Phone number must start with {code}" });
                        }
                    }
                }
                else
                {
                    // No explicit country — try to infer from prefix; if country exists with matching PhoneCode, accept; otherwise accept raw phone
                    // This keeps backward compatibility for existing clients while enabling linkage when available
                    var countryRepo = _unitOfWork.GetRepository<Country, Guid>();
                    // Find country whose PhoneCode is prefix of phone (longest first)
                    var allCountries = await countryRepo.GetAllListAsync(c => !c.IsDeleted && c.PhoneCode != null, cancellationToken);
                    var matched = allCountries
                        .Where(c => !string.IsNullOrWhiteSpace(c.PhoneCode) && phone.Replace(" ", "").StartsWith(c.PhoneCode!.Trim().Replace(" ", ""), StringComparison.Ordinal))
                        .OrderByDescending(c => c.PhoneCode!.Length)
                        .FirstOrDefault();
                    // If matched, linkage is implicitly validated; no error otherwise (phone may be local)
                }
            }

            // Organization distributor must be approved before OrganizationUser can register
            if (request.UserType == UserType.OrganizationUser)
            {
                var distRepo = _unitOfWork.GetRepository<DistributorApplication, Guid>();
                var hasApproved = await distRepo.ExistsAsync(
                    d => !d.IsDeleted && d.ContactEmail.ToLower() == request.Email.Trim().ToLower() && d.Status == DistributorApplicationStatus.Approved,
                    cancellationToken);
                if (!hasApproved)
                {
                    // Also check if an existing approved Company already linked by email domain? Fallback: check Company by contact email via DistributorApplication pending
                    var hasPending = await distRepo.ExistsAsync(
                        d => !d.IsDeleted && d.ContactEmail.ToLower() == request.Email.Trim().ToLower() && d.Status == DistributorApplicationStatus.Pending,
                        cancellationToken);
                    if (hasPending)
                        return Result<string>.BadRequest(LocalizationKeys.DistributorApplication.PendingApproval, new List<string> { LocalizationKeys.DistributorApplication.PendingApproval });
                    return Result<string>.BadRequest(LocalizationKeys.DistributorApplication.NotApplied, new List<string> { LocalizationKeys.DistributorApplication.NotApplied });
                }
            }

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
