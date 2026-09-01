using MediatR;
using Welco.Shared.Common.DTOs.Content;
using Welco.Shared.Results;

namespace Content.Services.API.Features.SupportTickets.Commands.CloseTicket
{
    public class CloseTicketCommand : IRequest<Result<SupportTicketDto>>
    {
        public Guid Id { get; set; }
    }
}
