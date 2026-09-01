using Content.Services.API.Common;
using MediatR;
using Welco.Shared.Common.DTOs.Content;
using Welco.Shared.Common.Extensions;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Localization;
using Welco.Shared.Results;
using SupportTicketEntity = Welco.Shared.Domain.Models.SupportTicket;

namespace Content.Services.API.Features.SupportTickets.Queries.GetTickets
{
    public class GetTicketsQueryHandler : IRequestHandler<GetTicketsQuery, PaginatedResult<SupportTicketDto>>
    {
        private readonly IUnitOfWork _uow;
        public GetTicketsQueryHandler(IUnitOfWork uow) => _uow = uow;

        public async Task<PaginatedResult<SupportTicketDto>> Handle(GetTicketsQuery request, CancellationToken cancellationToken)
        {
            var repo = _uow.GetRepository<SupportTicketEntity, Guid>();
            var q = repo.GetAll(t => !t.IsDeleted);
            if (!string.IsNullOrWhiteSpace(request.Status))
                q = q.Where(t => t.Status == request.Status);
            return await q.OrderByDescending(t => t.CreatedAt)
                .ToPaginatedListAsync(ContentDtoMapper.SupportTicketProjection, request.PageNumber, request.PageSize, LocalizationKeys.SupportTicket.ListFetched, cancellationToken);
        }
    }
}
