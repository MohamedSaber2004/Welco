using Content.Services.API.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Welco.Shared.Common.DTOs.Content;
using Welco.Shared.Common.Interfaces;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Localization;
using Welco.Shared.Results;
using SupportTicketEntity = Welco.Shared.Domain.Models.SupportTicket;

namespace Content.Services.API.Features.SupportTickets.Commands.CreateTicket
{
    public class CreateTicketCommandHandler : IRequestHandler<CreateTicketCommand, Result<SupportTicketDto>>
    {
        private readonly IUnitOfWork _uow;
        private readonly ICurrentUserService _currentUser;
        public CreateTicketCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser) { _uow = uow; _currentUser = currentUser; }

        public async Task<Result<SupportTicketDto>> Handle(CreateTicketCommand request, CancellationToken cancellationToken)
        {
            if (_currentUser.UserId == Guid.Empty) return Result<SupportTicketDto>.Unauthorized(LocalizationKeys.ExceptionMessages.Unauthorized);

            var currentUserId = _currentUser.UserId.ToString();
            var entity = new SupportTicketEntity
            {
                Id = Guid.NewGuid(),
                UserId = _currentUser.UserId,
                Subject = request.Subject.Trim(),
                Message = request.Message.Trim(),
                Status = "Open"
            };
            entity.MarkAsCreated(currentUserId);
            var repo = _uow.GetRepository<SupportTicketEntity, Guid>();
            await repo.AddAsync(entity, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);
            var dto = await repo.GetAll(t => !t.IsDeleted && t.Id == entity.Id)
                .Select(ContentDtoMapper.SupportTicketProjection)
                .FirstOrDefaultAsync(cancellationToken);
            return Result<SupportTicketDto>.Created(dto!, LocalizationKeys.SupportTicket.Created);
        }
    }
}
