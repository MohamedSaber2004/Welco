using MediatR;
using Welco.Shared.Common.DTOs.Content;
using Welco.Shared.Results;

namespace Content.Services.API.Features.SupportTickets.Queries.GetMyTickets
{
    public class GetMyTicketsQuery : IRequest<Result<List<SupportTicketDto>>>
    {
    }
}
