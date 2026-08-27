namespace Welco.Shared.Common.Options
{
    public class EmailSettings
    {
        public const string SectionName = "EmailSettings";

        public string Email { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Host { get; set; } = null!;
        public int Port { get; set; } = 587;
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
        public int VerificationCodeExpiryMinutes { get; set; } = 10;
        public bool EnableSsl { get; set; } = true;
    }
}
