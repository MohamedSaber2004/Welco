using Content.Services.API.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Welco.Shared.Common.DTOs.Content;
using Welco.Shared.Common.Interfaces;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Localization;
using Welco.Shared.Results;
using HelpCategoryEntity = Welco.Shared.Domain.Models.HelpCategory;

namespace Content.Services.API.Features.HelpCategories.Commands.UpdateHelpCategory
{
    public class UpdateHelpCategoryCommandHandler : IRequestHandler<UpdateHelpCategoryCommand, Result<HelpCategoryDto>>
    {
        private readonly IUnitOfWork _uow;
        private readonly ICurrentUserService _currentUser;

        public UpdateHelpCategoryCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
        {
            _uow = uow;
            _currentUser = currentUser;
        }

        public async Task<Result<HelpCategoryDto>> Handle(UpdateHelpCategoryCommand request, CancellationToken cancellationToken)
        {
            var repo = _uow.GetRepository<HelpCategoryEntity, Guid>();
            var entity = await repo.GetByIdAsync(request.Id, cancellationToken);
            if (entity == null || entity.IsDeleted)
                return Result<HelpCategoryDto>.NotFound(LocalizationKeys.HelpCategory.NotFound);

            var duplicate = await repo.ExistsAsync(c => !c.IsDeleted && c.Id != request.Id && c.Name.ToLower() == request.Name.Trim().ToLower(), cancellationToken);
            if (duplicate)
                return Result<HelpCategoryDto>.Conflict(LocalizationKeys.HelpCategory.AlreadyExists);

            var currentUserId = _currentUser.UserId != Guid.Empty ? _currentUser.UserId.ToString() : "System";
            entity.Name = request.Name.Trim();
            entity.Icon = request.Icon?.Trim();
            entity.MarkAsUpdated(currentUserId);
            repo.Update(entity);
            await _uow.SaveChangesAsync(cancellationToken);

            var dto = await repo.GetAll(c => !c.IsDeleted && c.Id == entity.Id)
                .Select(ContentDtoMapper.HelpCategoryProjection)
                .FirstOrDefaultAsync(cancellationToken);

            return Result<HelpCategoryDto>.Success(dto!, LocalizationKeys.HelpCategory.Updated);
        }
    }
}
