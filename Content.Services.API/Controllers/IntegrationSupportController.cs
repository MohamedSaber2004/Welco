using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Welco.Shared.Common.Attributes;
using Welco.Shared.Common.DTOs.Integration;
using Welco.Shared.Common.Extensions;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Controllers;
using Welco.Shared.Domain.Models;
using Welco.Shared.Results;

namespace Content.Services.API.Controllers
{
        [ServiceAuth]
    [ApiController]
    [IntegrationRouteName("Support")]
    [Route("api/integration/support")]
    public class IntegrationSupportController : AppControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public IntegrationSupportController(IMediator mediator, IUnitOfWork unitOfWork) : base(mediator)
        {
            _unitOfWork = unitOfWork;
        }

                [HttpGet("tickets")]
        public async Task<IActionResult> GetTickets([FromQuery] string? status, CancellationToken ct)
        {
            var repo = _unitOfWork.GetRepository<SupportTicket, Guid>();
            var query = repo.GetAll(t => !t.IsDeleted);

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(t => t.Status == status);

            var tickets = await query
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => new ExternalSupportTicketDto
                {
                    Id = t.Id,
                    UserId = t.UserId,
                    Subject = t.Subject,
                    Message = t.Message,
                    Status = t.Status,
                    Reply = t.Reply,
                    RepliedAt = t.RepliedAt,
                    CreatedAt = t.CreatedAt
                })
                .AsNoTracking()
                .ToListAsync(ct);

            return ToActionResult(Result<List<ExternalSupportTicketDto>>.Success(tickets));
        }

                [HttpGet("tickets/{id:guid}")]
        public async Task<IActionResult> GetTicketById([FromRoute] Guid id, CancellationToken ct)
        {
            var repo = _unitOfWork.GetRepository<SupportTicket, Guid>();
            var ticket = await repo.GetAll(t => !t.IsDeleted && t.Id == id)
                .Select(t => new ExternalSupportTicketDto
                {
                    Id = t.Id,
                    UserId = t.UserId,
                    Subject = t.Subject,
                    Message = t.Message,
                    Status = t.Status,
                    Reply = t.Reply,
                    RepliedAt = t.RepliedAt,
                    CreatedAt = t.CreatedAt
                })
                .AsNoTracking()
                .FirstOrDefaultAsync(ct);

            if (ticket == null)
                return ToActionResult(Result<ExternalSupportTicketDto>.NotFound("Support ticket not found."));

            return ToActionResult(Result<ExternalSupportTicketDto>.Success(ticket));
        }

                [HttpPost("tickets/{id:guid}/reply")]
        public async Task<IActionResult> ReplyTicket([FromRoute] Guid id, [FromBody] ReplySupportTicketRequest request, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(request?.Reply))
                return ToActionResult(Result<bool>.BadRequest("Reply text is required."));

            var repo = _unitOfWork.GetRepository<SupportTicket, Guid>();
            var ticket = await repo.GetByIdAsync(id, ct);
            if (ticket == null || ticket.IsDeleted)
                return ToActionResult(Result<bool>.NotFound("Support ticket not found."));

            ticket.Reply = request.Reply.Trim();
            ticket.RepliedAt = DateTime.UtcNow;
            ticket.Status = "Answered";
            ticket.MarkAsUpdated("integration-service");

            await _unitOfWork.SaveChangesAsync(ct);
            return ToActionResult(Result<bool>.Success(true));
        }

                [HttpPost("tickets/{id:guid}/close")]
        public async Task<IActionResult> CloseTicket([FromRoute] Guid id, CancellationToken ct)
        {
            var repo = _unitOfWork.GetRepository<SupportTicket, Guid>();
            var ticket = await repo.GetByIdAsync(id, ct);
            if (ticket == null || ticket.IsDeleted)
                return ToActionResult(Result<bool>.NotFound("Support ticket not found."));

            ticket.Status = "Closed";
            ticket.MarkAsUpdated("integration-service");

            await _unitOfWork.SaveChangesAsync(ct);
            return ToActionResult(Result<bool>.Success(true));
        }
    }
}
