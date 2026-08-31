using FluentValidation;
using Welco.Shared.Localization;

namespace Product.Services.API.Features.Currencies.Commands.CreateCurrency
{
    public class CreateCurrencyCommandValidator : AbstractValidator<CreateCurrencyCommand>
    {
        public CreateCurrencyCommandValidator()
        {
            RuleFor(x => x.NameEn)
                .NotEmpty().WithMessage(LocalizationKeys.Currency.NameEnRequired)
                .MaximumLength(100);

            RuleFor(x => x.NameAr)
                .NotEmpty().WithMessage(LocalizationKeys.Currency.NameArRequired)
                .MaximumLength(100);

            RuleFor(x => x.Code)
                .NotEmpty().WithMessage(LocalizationKeys.Currency.CodeRequired)
                .MaximumLength(10);

            RuleFor(x => x.Symbol)
                .NotEmpty().WithMessage(LocalizationKeys.Currency.SymbolRequired)
                .MaximumLength(10);
        }
    }
}
