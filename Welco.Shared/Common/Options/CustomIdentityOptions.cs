namespace Welco.Shared.Common.Options
{
    public class CustomIdentityOptions
    {
        public const string SectionName = "Identity";

        public bool RequiredDigit { get; set; } = true;
        public int RequiredLength { get; set; } = 6;
        public bool RequireLowercase { get; set; } = true;
        public int RequiredUniqueChars { get; set; } = 1;
        public bool RequireUppercase { get; set; } = true;
        public int MaxFailedAttempts { get; set; } = 5;
        public double LockoutTimeSpanInDays { get; set; } = 1;
        public bool RequireNonAlphanumeric { get; set; } = false;
        public string AllowedUserNameCharacters { get; set; } = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
        public bool RequireUniqueEmail { get; set; } = true;
        public bool RequireConfirmedEmail { get; set; } = false;
    }
}
