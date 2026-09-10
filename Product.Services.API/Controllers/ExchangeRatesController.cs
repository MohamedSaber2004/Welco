using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Product.Services.API.ProductRoutes;
using Welco.Shared.Common.Attributes;
using Welco.Shared.Common.DTOs.Products;
using Welco.Shared.Common.Interfaces;
using Welco.Shared.Controllers;
using Welco.Shared.Domain.Models;
using Welco.Shared.Enums;
using Welco.Shared.Results;

namespace Product.Services.API.Controllers
{
    [Route(ProductApiRoutes.ExchangeRates.Base)]
    public class ExchangeRatesController : AppControllerBase
    {
        private readonly IExchangeRateService _service;

        public ExchangeRatesController(IMediator mediator, IExchangeRateService service) : base(mediator) => _service = service;

                [HttpGet]
        [Route(ProductApiRoutes.ExchangeRates.Latest)]
        [AllowAnonymous]
        public async Task<IActionResult> GetLatest(CancellationToken ct)
        {
            try
            {
                var rates = await _service.GetLatestRatesAsync("USD", ct);
                return ToActionResult(Result<IReadOnlyCollection<ExchangeRateDto>>.Success(rates.ToList()));
            }
            catch (Exception ex)
            {
                return ToActionResult(Result<IReadOnlyCollection<ExchangeRateDto>>.Failure(ex.Message));
            }
        }

                [HttpGet]
        [Route(ProductApiRoutes.ExchangeRates.LatestByBase)]
        [AllowAnonymous]
        public async Task<IActionResult> GetLatestByBase([FromRoute] string baseCurrency, CancellationToken ct)
        {
            try
            {
                var rates = await _service.GetLatestRatesAsync(baseCurrency, ct);
                return ToActionResult(Result<IReadOnlyCollection<ExchangeRateDto>>.Success(rates.ToList()));
            }
            catch (Exception ex)
            {
                return ToActionResult(Result<IReadOnlyCollection<ExchangeRateDto>>.Failure(ex.Message));
            }
        }

                [HttpGet]
        [Route(ProductApiRoutes.ExchangeRates.Pair)]
        [AllowAnonymous]
        public async Task<IActionResult> GetPair([FromRoute] string from, [FromRoute] string to, CancellationToken ct)
        {
            try
            {
                var rate = await _service.GetLatestRateAsync(from, to, ct);
                if (rate == null) return ToActionResult(Result<ExchangeRateDto>.NotFound($"Rate not found for {from}->{to}"));
                return ToActionResult(Result<ExchangeRateDto>.Success(rate));
            }
            catch (Exception ex)
            {
                return ToActionResult(Result<ExchangeRateDto>.Failure(ex.Message));
            }
        }

                [HttpGet]
        [Route(ProductApiRoutes.ExchangeRates.Convert)]
        [AllowAnonymous]
        public async Task<IActionResult> Convert([FromQuery] string from, [FromQuery] string to, [FromQuery] decimal amount, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(from) || string.IsNullOrWhiteSpace(to))
                return ToActionResult(Result<ConversionResultDto>.BadRequest("from and to are required"));
            if (amount < 0)
                return ToActionResult(Result<ConversionResultDto>.BadRequest("amount must be >= 0"));
            try
            {
                var result = await _service.ConvertWithDetailsAsync(amount, from, to, ct);
                return ToActionResult(Result<ConversionResultDto>.Success(result));
            }
            catch (Exception ex)
            {
                return ToActionResult(Result<ConversionResultDto>.Failure(ex.Message));
            }
        }

                [HttpGet]
        [Route(ProductApiRoutes.ExchangeRates.History)]
        [AllowAnonymous]
        public async Task<IActionResult> GetHistory([FromRoute] string baseCurrency, [FromRoute] string date, CancellationToken ct)
        {
            if (!DateOnly.TryParse(date, out var d))
                return ToActionResult(Result<IReadOnlyCollection<ExchangeRateDto>>.BadRequest("Invalid date format, use yyyy-MM-dd"));
            try
            {
                var rates = await _service.GetHistoricalRatesAsync(baseCurrency, d, ct);
                return ToActionResult(Result<IReadOnlyCollection<ExchangeRateDto>>.Success(rates.ToList()));
            }
            catch (Exception ex)
            {
                return ToActionResult(Result<IReadOnlyCollection<ExchangeRateDto>>.Failure(ex.Message));
            }
        }

                [HttpPost]
        [Route(ProductApiRoutes.ExchangeRates.Sync)]
        [RoleAuthorize(UserType.Admin)]
        public async Task<IActionResult> Sync(CancellationToken ct)
        {
            var result = await _service.SyncLatestRatesAsync(ct);
            if (!result.Success) return ToActionResult(Result<ExchangeRateSyncResult>.Failure(result.ErrorMessage ?? "Sync failed", 502));
            return ToActionResult(Result<ExchangeRateSyncResult>.Success(result));
        }

        [HttpPost]
        [Route(ProductApiRoutes.ExchangeRates.SyncHistory)]
        [RoleAuthorize(UserType.Admin)]
        public async Task<IActionResult> SyncHistory([FromRoute] string date, CancellationToken ct)
        {
            if (!DateOnly.TryParse(date, out var d))
                return ToActionResult(Result<ExchangeRateSyncResult>.BadRequest("Invalid date"));
            var result = await _service.SyncHistoricalRatesAsync(d, ct);
            if (!result.Success) return ToActionResult(Result<ExchangeRateSyncResult>.Failure(result.ErrorMessage ?? "Sync failed", 502));
            return ToActionResult(Result<ExchangeRateSyncResult>.Success(result));
        }

                [HttpGet]
        [Route(ProductApiRoutes.ExchangeRates.SyncLogs)]
        [AllowAnonymous]
        public async Task<IActionResult> GetSyncLogs([FromQuery] int take = 10, CancellationToken ct = default)
        {
            var logs = await _service.GetSyncLogsAsync(take, ct);
            return ToActionResult(Result<IReadOnlyCollection<ExchangeRateSyncLog>>.Success(logs));
        }
    }
}
