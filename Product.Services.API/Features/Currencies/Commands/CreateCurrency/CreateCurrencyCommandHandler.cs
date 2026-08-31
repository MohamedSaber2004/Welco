using MediatR;
using Welco.Shared.Common.DTOs.Products;
using Welco.Shared.Common.Interfaces;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Localization;
using Welco.Shared.Results;
using CurrencyEntity = Welco.Shared.Domain.Models.Currency;

namespace Product.Services.API.Features.Currencies.Commands.CreateCurrency
{
    public class CreateCurrencyCommandHandler : IRequestHandler<CreateCurrencyCommand, Result<CurrencyDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public CreateCurrencyCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<Result<CurrencyDto>> Handle(CreateCurrencyCommand request, CancellationToken cancellationToken)
        {
            var code = request.Code.Trim().ToUpperInvariant();

            var currencyRepo = _unitOfWork.GetRepository<CurrencyEntity, Guid>();

            var codeExists = await currencyRepo.ExistsAsync(
                c => !c.IsDeleted && c.Code.ToLower() == code.ToLower(),
                cancellationToken);

            if (codeExists)
            {
                return Result<CurrencyDto>.Conflict(LocalizationKeys.Currency.CodeAlreadyExists);
            }

            var currentUserId = _currentUserService.UserId != Guid.Empty
                ? _currentUserService.UserId.ToString()
                : "System";

            var currency = CurrencyEntity.Create(
                request.NameEn.Trim(),
                request.NameAr.Trim(),
                code,
                request.Symbol.Trim(),
                currentUserId);

            await currencyRepo.AddAsync(currency, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<CurrencyDto>.Created(ToDto(currency), LocalizationKeys.Currency.Created);
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
