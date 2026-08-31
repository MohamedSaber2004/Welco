using Microsoft.AspNetCore.Identity;
using Welco.Shared.Common.Exceptions;
using Welco.Shared.Common.Interfaces;
using Welco.Shared.Enums;
using Welco.Shared.Localization;

namespace Welco.Shared.Domain.Models
{
    public class ApplicationUser : IdentityUser<Guid>, IBaseEntity<Guid>
    {
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt {get; set;}
        public string CreatedBy {get; set;} = null!;
        public string? UpdatedBy { get; set; }
        public string? DeletedBy {get; set; }
        public bool IsDeleted { get; set; }
        public bool IsActive {get; set;}

        public string FullName { get; set; } = null!;
        public string? ProfilePictureName { get; set; }
        public string? PasswordResetToken { get; set; }
        public DateTime? PasswordResetTokenExpiry { get; set; }
        public string? EmailConfirmationOtp { get; set; }
        public DateTime? EmailConfirmationOtpExpiry { get; set; }
        public AppLanguage Language { get; set; }
        public UserType UserType { get; set; } = UserType.OrganizationUser;
        public Guid? CompanyId { get; set; }
        public virtual Company? Company { get; set; }

        public virtual ICollection<UserAddress> Addresses { get; set; } = new List<UserAddress>();

        public void MarkAsCreated(string createdBy)
        {
            CreatedAt = DateTime.UtcNow;
            CreatedBy = createdBy;
            IsActive = true;
            IsDeleted = false;
        }

        public void MarkAsUpdated(string updatedBy)
        {
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = updatedBy;
        }

        public void MarkAsDeleted(string deletedBy)
        {
            IsDeleted = true;
            IsActive = false;
            DeletedAt = DateTime.UtcNow;
            DeletedBy = deletedBy;
        }

        public void Activate(string updatedBy)
        {
            IsActive = true;
            IsDeleted = false;
            MarkAsUpdated(updatedBy);
        }

        public void Deactivate(string updatedBy)
        {
            IsActive = false;
            MarkAsUpdated(updatedBy);
        }

        public void UpdateProfile(string fullName, string? profilePictureName, string updatedBy)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                throw new BadRequestException(LocalizationKeys.Auth.FullNameRequired);

            FullName = fullName;
            ProfilePictureName = profilePictureName;
            MarkAsUpdated(updatedBy);
        }

        public void SetLanguage(AppLanguage language, string updatedBy)
        {
            Language = language;
            MarkAsUpdated(updatedBy);
        }

        public void SetUserType(UserType userType, string updatedBy)
        {
            UserType = userType;
            MarkAsUpdated(updatedBy);
        }

        public void RequestPasswordReset(string token, DateTime expiry)
        {
            if (string.IsNullOrWhiteSpace(token))
                throw new BadRequestException(LocalizationKeys.Auth.TokenRequired);

            if (expiry <= DateTime.UtcNow)
                throw new BadRequestException(LocalizationKeys.Auth.TokenExpiryInFuture);

            PasswordResetToken = token.Trim();
            PasswordResetTokenExpiry = expiry;
        }

        public bool ValidatePasswordResetToken(string token)
        {
            return !string.IsNullOrWhiteSpace(token)
                && string.Equals(PasswordResetToken?.Trim(), token.Trim(), StringComparison.Ordinal)
                && PasswordResetTokenExpiry.HasValue
                && PasswordResetTokenExpiry.Value > DateTime.UtcNow;
        }

        public void ClearPasswordResetToken()
        {
            PasswordResetToken = null;
            PasswordResetTokenExpiry = null;
        }

        public void SetEmailConfirmationOtp(string otp, DateTime expiry)
        {
            if (string.IsNullOrWhiteSpace(otp))
                throw new BadRequestException(LocalizationKeys.Auth.OtpCodeRequired);

            if (expiry <= DateTime.UtcNow)
                throw new BadRequestException(LocalizationKeys.Auth.TokenExpiryInFuture);

            EmailConfirmationOtp = otp.Trim();
            EmailConfirmationOtpExpiry = expiry;
        }

        public bool ValidateEmailConfirmationOtp(string otp)
        {
            return !string.IsNullOrWhiteSpace(otp)
                && string.Equals(EmailConfirmationOtp?.Trim(), otp.Trim(), StringComparison.Ordinal)
                && EmailConfirmationOtpExpiry.HasValue
                && EmailConfirmationOtpExpiry.Value > DateTime.UtcNow;
        }

        public void ClearEmailConfirmationOtp()
        {
            EmailConfirmationOtp = null;
            EmailConfirmationOtpExpiry = null;
        }
    }
}
