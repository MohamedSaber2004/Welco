using Microsoft.AspNetCore.Identity;
using Welco.Shared.Common.Interfaces;
using Welco.Shared.Enums;

namespace Auth.Services.API.Domain.Models
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

        public string FullName { get; private set; } = null!;
        public string? ProfilePictureName { get; private set; }
        public string? PasswordResetToken { get; private set; }
        public DateTime? PasswordResetTokenExpiry { get; private set; }
        public AppLanguage Language { get; private set; }
        public UserType UserType { get; private set; } = UserType.User;

        public void MarkAsCreated(string createdBy)
        {
            CreatedAt = DateTime.Now;
            CreatedBy = createdBy;
            IsActive = true;
            IsDeleted = false;
        }

        public void MarkAsUpdated(string updatedBy)
        {
            UpdatedAt = DateTime.Now;
            UpdatedBy = updatedBy;
        }

        public void MarkAsDeleted(string deletedBy)
        {
            IsDeleted = true;
            IsActive = false;
            DeletedAt = DateTime.Now;
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
                throw new ArgumentException("Full name cannot be empty.", nameof(fullName));

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
                throw new ArgumentException("Reset token cannot be empty.", nameof(token));

            if (expiry <= DateTime.Now)
                throw new ArgumentException("Reset token expiry must be in the future.", nameof(expiry));

            PasswordResetToken = token;
            PasswordResetTokenExpiry = expiry;
        }

        public bool ValidatePasswordResetToken(string token)
        {
            return !string.IsNullOrWhiteSpace(token)
                && PasswordResetToken == token
                && PasswordResetTokenExpiry.HasValue
                && PasswordResetTokenExpiry.Value > DateTime.Now;
        }

        public void ClearPasswordResetToken()
        {
            PasswordResetToken = null;
            PasswordResetTokenExpiry = null;
        }
    }
}
