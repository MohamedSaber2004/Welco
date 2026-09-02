using FluentValidation;

namespace Content.Services.API.Features.TradeShows.Commands.CreateTradeShow
{
    public class CreateTradeShowCommandValidator : AbstractValidator<CreateTradeShowCommand>
    {
        public CreateTradeShowCommandValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Location).NotEmpty().MaximumLength(300);
            RuleFor(x => x.EndDate).GreaterThanOrEqualTo(x => x.StartDate);
        }
    }
}
