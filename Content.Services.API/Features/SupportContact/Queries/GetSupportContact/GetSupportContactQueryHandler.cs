using Content.Services.API.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Welco.Shared.Common.DTOs.Content;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Localization;
using Welco.Shared.Results;
using SupportContactEntity = Welco.Shared.Domain.Models.SupportContact;

namespace Content.Services.API.Features.SupportContact.Queries.GetSupportContact
{
    public class GetSupportContactQueryHandler : IRequestHandler<GetSupportContactQuery, Result<SupportContactDto>>
    {
        private readonly IUnitOfWork _uow;

        public GetSupportContactQueryHandler(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<Result<SupportContactDto>> Handle(GetSupportContactQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var repo = _uow.GetRepository<SupportContactEntity, Guid>();
                var contact = await repo.GetAll(c => !c.IsDeleted)
                    .AsNoTracking()
                    .OrderByDescending(c => c.UpdatedAt ?? c.CreatedAt)
                    .Select(ContentDtoMapper.SupportContactProjection)
                    .FirstOrDefaultAsync(cancellationToken);

                if (contact != null)
                {
                    return Result<SupportContactDto>.Success(contact, LocalizationKeys.SupportContact.Fetched);
                }

                // If not found in DB, seed a default record
                var defaultEntity = new SupportContactEntity
                {
                    SupportEmail = "support@welco.health",
                    PhoneNumber = "+971 50 000 0000",
                    WhatsAppNumber = "+971500000000",
                    WorkingHours = "Mon - Fri: 8:00 AM - 6:00 PM (GST)"
                };
                defaultEntity.MarkAsCreated("System");

                await repo.AddAsync(defaultEntity, cancellationToken);
                await _uow.SaveChangesAsync(cancellationToken);

                var dto = new SupportContactDto
                {
                    Id = defaultEntity.Id,
                    SupportEmail = defaultEntity.SupportEmail,
                    PhoneNumber = defaultEntity.PhoneNumber,
                    WhatsAppNumber = defaultEntity.WhatsAppNumber,
                    WorkingHours = defaultEntity.WorkingHours,
                    UpdatedAt = defaultEntity.CreatedAt
                };

                return Result<SupportContactDto>.Success(dto, LocalizationKeys.SupportContact.Fetched);
            }
            catch
            {
                // Fallback graceful default if DB table not yet migrated
                var fallback = new SupportContactDto
                {
                    Id = Guid.Empty,
                    SupportEmail = "support@welco.health",
                    PhoneNumber = "+971 50 000 0000",
                    WhatsAppNumber = "+971500000000",
                    WorkingHours = "Mon - Fri: 8:00 AM - 6:00 PM (GST)",
                    UpdatedAt = DateTime.UtcNow
                };
                return Result<SupportContactDto>.Success(fallback, LocalizationKeys.SupportContact.Fetched);
            }
        }
    }
}
