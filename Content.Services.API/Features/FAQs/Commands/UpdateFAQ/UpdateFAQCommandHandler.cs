using Content.Services.API.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Welco.Shared.Common.DTOs.Content;
using Welco.Shared.Common.Interfaces;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Localization;
using Welco.Shared.Results;
using FAQEntity = Welco.Shared.Domain.Models.FAQItem;

namespace Content.Services.API.Features.FAQs.Commands.UpdateFAQ
{
    public class UpdateFAQCommandHandler : IRequestHandler<UpdateFAQCommand, Result<FAQItemDto>>
    {
        private readonly IUnitOfWork _uow;
        private readonly ICurrentUserService _currentUser;
        public UpdateFAQCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser) { _uow = uow; _currentUser = currentUser; }

        public async Task<Result<FAQItemDto>> Handle(UpdateFAQCommand request, CancellationToken cancellationToken)
        {
            var repo = _uow.GetRepository<FAQEntity, Guid>();
            var entity = await repo.GetByIdAsync(request.Id, cancellationToken);
            if (entity == null || entity.IsDeleted) return Result<FAQItemDto>.NotFound(LocalizationKeys.FAQ.NotFound);
            var currentUserId = _currentUser.UserId != Guid.Empty ? _currentUser.UserId.ToString() : "System";
            entity.Question = request.Question.Trim();
            entity.Answer = request.Answer.Trim();
            entity.SortOrder = request.SortOrder;
            entity.MarkAsUpdated(currentUserId);
            repo.Update(entity);
            await _uow.SaveChangesAsync(cancellationToken);
            var dto = await repo.GetAll(f => !f.IsDeleted && f.Id == entity.Id)
                .Select(ContentDtoMapper.FAQProjection)
                .FirstOrDefaultAsync(cancellationToken);
            return Result<FAQItemDto>.Success(dto!, LocalizationKeys.FAQ.Updated);
        }
    }
}
