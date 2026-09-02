using MediatR;
using Welco.Shared.Common.DTOs.UserManagement;
using Welco.Shared.Results;

namespace UserManamgent.Service.API.Features.AuditLogs.Queries.GetAuditLogs.GetAuditLogById
{
    public class GetAuditLogByIdQuery : IRequest<Result<AuditLogDto>>
    {
        public Guid Id { get; set; }
    }
}
