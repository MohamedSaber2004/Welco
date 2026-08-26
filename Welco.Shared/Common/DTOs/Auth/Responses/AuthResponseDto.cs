using Welco.Shared.Enums;

namespace Welco.Shared.Common.DTOs.Auth.Responses
{
    public class AuthResponseDto
    {
        public Guid UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? UserName { get; set; }
        public UserType UserType { get; set; }
        public AppLanguage Language { get; set; }
        public IList<string> Roles { get; set; } = new List<string>();
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime RefreshTokenExpiryTime { get; set; }
    }
}
