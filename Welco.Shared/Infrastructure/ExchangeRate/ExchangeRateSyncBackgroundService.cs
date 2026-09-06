using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Welco.Shared.Common.Interfaces;
using Welco.Shared.Common.Options;

namespace Welco.Shared.Infrastructure.ExchangeRate
{
    public class ExchangeRateSyncBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ExchangeRateSettings _settings;
        private readonly ILogger<ExchangeRateSyncBackgroundService> _logger;

        public ExchangeRateSyncBackgroundService(
            IServiceScopeFactory scopeFactory,
            IOptions<ExchangeRateSettings> options,
            ILogger<ExchangeRateSyncBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _settings = options.Value;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var intervalHours = _settings.SyncIntervalHours > 0 ? _settings.SyncIntervalHours : 24;
            _logger.LogInformation("ExchangeRate sync background service started, interval {Hours}h, base {Base}", intervalHours, _settings.BaseCurrency);

            // Initial delay to let app start
            try { await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken); } catch { return; }

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var svc = scope.ServiceProvider.GetRequiredService<IExchangeRateService>();
                    var result = await svc.SyncLatestRatesAsync(stoppingToken);
                    if (result.Success)
                        _logger.LogInformation("Background sync success {Base} {Count} rates {Date}", result.BaseCurrency, result.RatesCount, result.RateDate);
                    else
                        _logger.LogWarning("Background sync failed {Base}: {Error}", result.BaseCurrency, result.ErrorMessage);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unhandled error in ExchangeRate background sync");
                }

                try
                {
                    await Task.Delay(TimeSpan.FromHours(intervalHours), stoppingToken);
                }
                catch (TaskCanceledException) { break; }
            }

            _logger.LogInformation("ExchangeRate sync background service stopped");
        }
    }
}
