using Microsoft.Extensions.Logging;
using Welco.Shared.Common.Interfaces;

namespace Product.Services.API.Jobs
{
    public class ExchangeRateSyncJob
    {
        private readonly IExchangeRateService _exchangeRateService;
        private readonly ILogger<ExchangeRateSyncJob> _logger;

        public ExchangeRateSyncJob(
            IExchangeRateService exchangeRateService,
            ILogger<ExchangeRateSyncJob> logger)
        {
            _exchangeRateService = exchangeRateService;
            _logger = logger;
        }

        public async Task ExecuteAsync()
        {
            _logger.LogInformation("Hangfire recurring job: Starting exchange rate sync...");
            var result = await _exchangeRateService.SyncLatestRatesAsync(CancellationToken.None);

            if (!result.Success)
            {
                _logger.LogError("Hangfire exchange rate sync failed: {ErrorMessage}", result.ErrorMessage);
                throw new InvalidOperationException($"Exchange rate sync failed: {result.ErrorMessage}");
            }

            _logger.LogInformation("Hangfire exchange rate sync completed successfully. Base: {BaseCurrency}, Rates: {RatesCount}, Source: {Source}, Date: {Date}",
                result.BaseCurrency, result.RatesCount, result.Source, result.RateDate);
        }
    }
}
