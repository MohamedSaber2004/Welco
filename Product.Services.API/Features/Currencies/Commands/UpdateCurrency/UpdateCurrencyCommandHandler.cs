using MediatR;
using Welco.Shared.Common.DTOs.Products;
using Welco.Shared.Common.Interfaces;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Localization;
using Welco.Shared.Results;
using CurrencyEntity = Welco.Shared.Domain.Models.Currency;

namespace Product.Services.API.Features.Currencies.Commands.UpdateCurrency
{
    public class UpdateCurrencyCommandHandler : IRequestHandler<UpdateCurrencyCommand, Result<CurrencyDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public UpdateCurrencyCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<Result<CurrencyDto>> Handle(UpdateCurrencyCommand request, CancellationToken cancellationToken)
        {
            var currencyRepo = _unitOfWork.GetRepository<CurrencyEntity, Guid>();
            var currency = await currencyRepo.GetByIdAsync(request.Id, cancellationToken);

            if (currency == null || currency.IsDeleted)
            {
                return Result<CurrencyDto>.NotFound(LocalizationKeys.Currency.NotFound);
            }

            var code = request.Code.Trim().ToUpperInvariant();
            if (!string.Equals(currency.Code, code, StringComparison.OrdinalIgnoreCase))
            {
                var codeExists = await currencyRepo.ExistsAsync(
                    c => !c.IsDeleted && c.Id != request.Id && c.Code.ToLower() == code.ToLower(),
                    cancellationToken);

                if (codeExists)
                {
                    return Result<CurrencyDto>.Conflict(LocalizationKeys.Currency.CodeAlreadyExists);
                }
            }

            var currentUserId = _currentUserService.UserId != Guid.Empty
                ? _currentUserService.UserId.ToString()
                : "System";

            currency.Update(
                request.NameEn.Trim(),
                request.NameAr.Trim(),
                code,
                request.Symbol.Trim(),
                currentUserId);

            if (request.IsActive.HasValue)
            {
                currency.SetActiveState(request.IsActive.Value, currentUserId);
            }

            currencyRepo.Update(currency);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<CurrencyDto>.Success(ToDto(currency), LocalizationKeys.Currency.Updated);
        }

        private static CurrencyDto ToDto(CurrencyEntity currency)
        {
            return new CurrencyDto
            {
                Id = currency.Id,
                NameEn = currency.NameEn,
                NameAr = currency.NameAr,
                Code = currency.Code,
                Symbol = currency.Symbol,
                IsActive = currency.IsActive,
                CreatedAt = currency.CreatedAt,
                UpdatedAt = currency.UpdatedAt
            };
        }
    }
}
