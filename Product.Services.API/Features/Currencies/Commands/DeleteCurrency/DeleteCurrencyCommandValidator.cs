using FluentValidation;
using Welco.Shared.Localization;

namespace Product.Services.API.Features.Currencies.Commands.DeleteCurrency
{
    public class DeleteCurrencyCommandValidator : AbstractValidator<DeleteCurrencyCommand>
    {
        public DeleteCurrencyCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage(LocalizationKeys.Currency.CurrencyIdRequired);
        }
    }
}
