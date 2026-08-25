namespace Welco.API.Options
{
    public class RateLimitingOptions
    {
        public const string SectionName = "RateLimiting";

        public int PermitLimit { get; set; } 
        public int WindowSeconds { get; set; } 
        public int QueueLimit { get; set; }
    }
}
