using FluentValidation;
using Welco.Shared.Localization;

namespace Content.Services.API.Features.SupportTickets.Commands.CreateTicket
{
    public class CreateTicketCommandValidator : AbstractValidator<CreateTicketCommand>
    {
        public CreateTicketCommandValidator()
        {
            RuleFor(x => x.Subject).NotEmpty().WithMessage(LocalizationKeys.SupportTicket.SubjectRequired).MaximumLength(300);
            RuleFor(x => x.Message).NotEmpty().WithMessage(LocalizationKeys.SupportTicket.MessageRequired);
        }
    }
}
