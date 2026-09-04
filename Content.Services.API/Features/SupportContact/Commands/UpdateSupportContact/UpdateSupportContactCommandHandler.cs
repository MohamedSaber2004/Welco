using Content.Services.API.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Welco.Shared.Common.DTOs.Content;
using Welco.Shared.Common.Interfaces;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Localization;
using Welco.Shared.Results;
using SupportContactEntity = Welco.Shared.Domain.Models.SupportContact;

namespace Content.Services.API.Features.SupportContact.Commands.UpdateSupportContact
{
    public class UpdateSupportContactCommandHandler : IRequestHandler<UpdateSupportContactCommand, Result<SupportContactDto>>
    {
        private readonly IUnitOfWork _uow;
        private readonly ICurrentUserService _currentUser;

        public UpdateSupportContactCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
        {
            _uow = uow;
            _currentUser = currentUser;
        }

        public async Task<Result<SupportContactDto>> Handle(UpdateSupportContactCommand request, CancellationToken cancellationToken)
        {
            var repo = _uow.GetRepository<SupportContactEntity, Guid>();
            var currentUserId = _currentUser.UserId != Guid.Empty ? _currentUser.UserId.ToString() : "Admin";

            var entity = await repo.GetAll(c => !c.IsDeleted)
                .OrderByDescending(c => c.UpdatedAt ?? c.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (entity != null)
            {
                entity.SupportEmail = request.SupportEmail.Trim();
                entity.PhoneNumber = request.PhoneNumber.Trim();
                entity.WhatsAppNumber = request.WhatsAppNumber.Trim();
                entity.WorkingHours = request.WorkingHours?.Trim();
                entity.MarkAsUpdated(currentUserId);
                repo.Update(entity);
            }
            else
            {
                entity = new SupportContactEntity
                {
                    SupportEmail = request.SupportEmail.Trim(),
                    PhoneNumber = request.PhoneNumber.Trim(),
                    WhatsAppNumber = request.WhatsAppNumber.Trim(),
                    WorkingHours = request.WorkingHours?.Trim()
                };
                entity.MarkAsCreated(currentUserId);
                await repo.AddAsync(entity, cancellationToken);
            }

            await _uow.SaveChangesAsync(cancellationToken);

            var dto = await repo.GetAll(c => !c.IsDeleted && c.Id == entity.Id)
                .Select(ContentDtoMapper.SupportContactProjection)
                .FirstOrDefaultAsync(cancellationToken) ?? new SupportContactDto
                {
                    Id = entity.Id,
                    SupportEmail = entity.SupportEmail,
                    PhoneNumber = entity.PhoneNumber,
                    WhatsAppNumber = entity.WhatsAppNumber,
                    WorkingHours = entity.WorkingHours,
                    UpdatedAt = entity.UpdatedAt ?? entity.CreatedAt
                };

            return Result<SupportContactDto>.Success(dto, LocalizationKeys.SupportContact.Updated);
        }
    }
}
