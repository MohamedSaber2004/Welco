using MediatR;
using Welco.Shared.Common.DTOs.Content;
using Welco.Shared.Results;

namespace Content.Services.API.Features.SupportTickets.Queries.GetTicketById
{
    public class GetTicketByIdQuery : IRequest<Result<SupportTicketDto>>
    {
        public Guid Id { get; set; }
    }
}
