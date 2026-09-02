using MediatR;
using Microsoft.EntityFrameworkCore;
using Welco.Shared.Common.Extensions;
using Welco.Shared.Common.DTOs.UserManagement;
using Welco.Shared.Domain.Models;
using Welco.Shared.Localization;
using Welco.Shared.Results;

namespace UserManamgent.Service.API.Features.AuditLogs.Queries.GetAuditLogs.GetAuditLogs
{
    public class GetAuditLogsQueryHandler : IRequestHandler<GetAuditLogsQuery, PaginatedResult<AuditLogDto>>
    {
        private readonly Welco.Shared.Persistance.WelcoDbContext _context;

        public GetAuditLogsQueryHandler(Welco.Shared.Persistance.WelcoDbContext context)
        {
            _context = context;
        }

        public async Task<PaginatedResult<AuditLogDto>> Handle(GetAuditLogsQuery request, CancellationToken cancellationToken)
        {
            var query = _context.AuditLogs.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(request.EntityName))
                query = query.Where(a => a.EntityName == request.EntityName);

            if (!string.IsNullOrWhiteSpace(request.Action))
                query = query.Where(a => a.Action == request.Action);

            if (!string.IsNullOrWhiteSpace(request.PerformedBy))
                query = query.Where(a => a.PerformedBy != null && a.PerformedBy.Contains(request.PerformedBy));

            if (request.StartDate.HasValue)
                query = query.Where(a => a.CreatedAt >= request.StartDate.Value);

            if (request.EndDate.HasValue)
                query = query.Where(a => a.CreatedAt <= request.EndDate.Value);

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var term = request.SearchTerm.Trim();
                query = query.Where(a =>
                    a.EntityName.Contains(term) ||
                    a.EntityId.Contains(term) ||
                    a.Action.Contains(term) ||
                    (a.PerformedBy != null && a.PerformedBy.Contains(term)) ||
                    (a.Details != null && a.Details.Contains(term)));
            }

            return await query
                .OrderByDescending(a => a.CreatedAt)
                .ToPaginatedListAsync(
                    a => new AuditLogDto
                    {
                        Id = a.Id,
                        EntityName = a.EntityName,
                        EntityId = a.EntityId,
                        Action = a.Action,
                        Details = a.Details,
                        PerformedBy = a.PerformedBy ?? "System",
                        CreatedAt = a.CreatedAt,
                    },
                    request.PageNumber,
                    request.PageSize,
                    LocalizationKeys.AuditLog.ListFetched,
                    cancellationToken: cancellationToken);
        }
    }
}
