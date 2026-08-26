namespace Welco.Shared.Common.Options
{
    public class JwtSettings
    {
        public const string SectionName = "JwtSettings";

        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public int ExpiryInMinutes { get; set; } = 60;
        public int RefreshTokenExpiryDays { get; set; } = 30;
        public string Secret { get; set; } = string.Empty;
    }
}
