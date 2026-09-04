using FluentValidation;
using Welco.Shared.Localization;

namespace Content.Services.API.Features.TradeShows.Commands.CreateTradeShow
{
    public class CreateTradeShowCommandValidator : AbstractValidator<CreateTradeShowCommand>
    {
        public CreateTradeShowCommandValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(LocalizationKeys.TradeShow.NameRequired).MaximumLength(200);
            RuleFor(x => x.Location).NotEmpty().WithMessage(LocalizationKeys.TradeShow.LocationRequired).MaximumLength(300);
            RuleFor(x => x.EndDate).GreaterThanOrEqualTo(x => x.StartDate).WithMessage(LocalizationKeys.TradeShow.EndDateMustBeAfterStartDate);
        }
    }
}
