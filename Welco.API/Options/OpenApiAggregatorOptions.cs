namespace Welco.API.Options
{
    public class OpenApiAggregatorOptions
    {
        public const string SectionName = "OpenApiAggregator";

        public int TimeoutSeconds { get; set; } = 15;
        public int RetryCount { get; set; } = 1;
    }
}
