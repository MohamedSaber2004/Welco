using MediatR;
using Microsoft.EntityFrameworkCore;
using Welco.Shared.Common.DTOs.UserManagement;
using Welco.Shared.Domain.Models;
using Welco.Shared.Localization;
using Welco.Shared.Results;

namespace UserManamgent.Service.API.Features.AuditLogs.Queries.GetAuditLogs.GetAuditLogById
{
    public class GetAuditLogByIdQueryHandler : IRequestHandler<GetAuditLogByIdQuery, Result<AuditLogDto>>
    {
        private readonly Welco.Shared.Persistance.WelcoDbContext _context;

        public GetAuditLogByIdQueryHandler(Welco.Shared.Persistance.WelcoDbContext context)
        {
            _context = context;
        }

        public async Task<Result<AuditLogDto>> Handle(GetAuditLogByIdQuery request, CancellationToken cancellationToken)
        {
            var log = await _context.AuditLogs
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == request.Id && a.Id != Guid.Empty, cancellationToken);

            if (log == null)
                return Result<AuditLogDto>.Failure(LocalizationKeys.AuditLog.NotFound);

            return Result<AuditLogDto>.Success(new AuditLogDto
            {
                Id = log.Id,
                EntityName = log.EntityName,
                EntityId = log.EntityId,
                Action = log.Action,
                Details = log.Details,
                PerformedBy = log.PerformedBy ?? "System",
                CreatedAt = log.CreatedAt,
            });
        }
    }
}
