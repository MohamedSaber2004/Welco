using FluentValidation;
using Welco.Shared.Localization;

namespace Content.Services.API.Features.SupportTickets.Commands.ReplyTicket
{
    public class ReplyTicketCommandValidator : AbstractValidator<ReplyTicketCommand>
    {
        public ReplyTicketCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty().WithMessage(LocalizationKeys.SupportTicket.TicketIdRequired);
            RuleFor(x => x.Reply).NotEmpty().WithMessage(LocalizationKeys.SupportTicket.ReplyRequired);
        }
    }
}
