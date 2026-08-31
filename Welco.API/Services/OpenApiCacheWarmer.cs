namespace Welco.API.Services
{
    public class OpenApiCacheWarmer : BackgroundService
    {
        private readonly OpenApiAggregatorService _aggregator;
        private readonly ILogger<OpenApiCacheWarmer> _logger;

        public OpenApiCacheWarmer(OpenApiAggregatorService aggregator, ILogger<OpenApiCacheWarmer> logger)
        {
            _aggregator = aggregator;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Give downstream services a moment to finish starting before polling them.
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                return;
            }

            try
            {
                await _aggregator.WarmUpAsync(stoppingToken);
                _logger.LogInformation("OpenAPI schema cache pre-warmed for all microservices.");
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // shutdown
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "OpenAPI schema cache pre-warm completed with errors.");
            }
        }
    }
}
