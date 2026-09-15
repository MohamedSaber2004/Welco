using Welco.Shared.Common.DTOs.Products;

namespace Welco.Shared.Common.Interfaces
{
    public interface IExchangeRateService
    {
        Task<ExchangeRateDto?> GetLatestRateAsync(string fromCurrency, string toCurrency, CancellationToken cancellationToken);
        Task<decimal> ConvertAsync(decimal amount, string fromCurrency, string toCurrency, CancellationToken cancellationToken);
        Task<ConversionResultDto> ConvertWithDetailsAsync(decimal amount, string fromCurrency, string toCurrency, CancellationToken cancellationToken);
        Task<CartTotalResultDto> ConvertCartTotalAsync(ConvertCartTotalRequest request, CancellationToken cancellationToken);
        Task<IReadOnlyCollection<ExchangeRateDto>> GetLatestRatesAsync(string baseCurrency, CancellationToken cancellationToken);
    }
}
