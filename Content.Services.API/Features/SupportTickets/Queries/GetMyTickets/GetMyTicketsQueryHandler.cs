using Content.Services.API.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Welco.Shared.Common.DTOs.Content;
using Welco.Shared.Common.Interfaces;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Localization;
using Welco.Shared.Results;
using SupportTicketEntity = Welco.Shared.Domain.Models.SupportTicket;

namespace Content.Services.API.Features.SupportTickets.Queries.GetMyTickets
{
    public class GetMyTicketsQueryHandler : IRequestHandler<GetMyTicketsQuery, Result<List<SupportTicketDto>>>
    {
        private readonly IUnitOfWork _uow;
        private readonly ICurrentUserService _currentUser;
        public GetMyTicketsQueryHandler(IUnitOfWork uow, ICurrentUserService currentUser) { _uow = uow; _currentUser = currentUser; }

        public async Task<Result<List<SupportTicketDto>>> Handle(GetMyTicketsQuery request, CancellationToken cancellationToken)
        {
            var repo = _uow.GetRepository<SupportTicketEntity, Guid>();
            var uid = _currentUser.UserId;
            var list = await repo.GetAll(t => !t.IsDeleted && t.UserId == uid)
                .OrderByDescending(t => t.CreatedAt)
                .Select(ContentDtoMapper.SupportTicketProjection)
                .ToListAsync(cancellationToken);
            return Result<List<SupportTicketDto>>.Success(list, LocalizationKeys.SupportTicket.ListFetched);
        }
    }
}
