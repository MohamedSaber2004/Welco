using MediatR;
using Welco.Shared.Common.DTOs.Content;
using Welco.Shared.Results;

namespace Content.Services.API.Features.SupportTickets.Commands.ReplyTicket
{
    public class ReplyTicketCommand : IRequest<Result<SupportTicketDto>>
    {
        public Guid Id { get; set; }
        public string Reply { get; set; } = string.Empty;
    }
}
