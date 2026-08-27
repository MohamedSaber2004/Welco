using Welco.Shared.Enums;

namespace Welco.Shared.Common.DTOs.UserManagement
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public string? ProfilePictureName { get; set; }
        public UserType UserType { get; set; }
        public AppLanguage Language { get; set; }
        public bool IsActive { get; set; }
        public bool IsEmailConfirmed { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public IList<string> Roles { get; set; } = new List<string>();
    }
}
