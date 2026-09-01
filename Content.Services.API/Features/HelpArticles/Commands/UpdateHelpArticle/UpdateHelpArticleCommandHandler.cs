using Content.Services.API.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Welco.Shared.Common.DTOs.Content;
using Welco.Shared.Common.Interfaces;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Localization;
using Welco.Shared.Results;
using HelpArticleEntity = Welco.Shared.Domain.Models.HelpArticle;
using HelpCategoryEntity = Welco.Shared.Domain.Models.HelpCategory;

namespace Content.Services.API.Features.HelpArticles.Commands.UpdateHelpArticle
{
    public class UpdateHelpArticleCommandHandler : IRequestHandler<UpdateHelpArticleCommand, Result<HelpArticleDto>>
    {
        private readonly IUnitOfWork _uow;
        private readonly ICurrentUserService _currentUser;
        public UpdateHelpArticleCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser) { _uow = uow; _currentUser = currentUser; }

        public async Task<Result<HelpArticleDto>> Handle(UpdateHelpArticleCommand request, CancellationToken cancellationToken)
        {
            var repo = _uow.GetRepository<HelpArticleEntity, Guid>();
            var entity = await repo.GetByIdAsync(request.Id, cancellationToken);
            if (entity == null || entity.IsDeleted) return Result<HelpArticleDto>.NotFound(LocalizationKeys.HelpArticle.NotFound);

            var catRepo = _uow.GetRepository<HelpCategoryEntity, Guid>();
            var catExists = await catRepo.ExistsAsync(c => !c.IsDeleted && c.Id == request.CategoryId, cancellationToken);
            if (!catExists) return Result<HelpArticleDto>.NotFound(LocalizationKeys.HelpCategory.NotFound);

            var slug = request.Slug.Trim().ToLowerInvariant();
            var slugExists = await repo.ExistsAsync(a => !a.IsDeleted && a.Id != request.Id && a.Slug.ToLower() == slug, cancellationToken);
            if (slugExists) return Result<HelpArticleDto>.Conflict(LocalizationKeys.HelpArticle.SlugAlreadyExists);

            var currentUserId = _currentUser.UserId != Guid.Empty ? _currentUser.UserId.ToString() : "System";
            entity.CategoryId = request.CategoryId;
            entity.Title = request.Title.Trim();
            entity.Body = request.Body.Trim();
            entity.Slug = slug;
            entity.MarkAsUpdated(currentUserId);
            repo.Update(entity);
            await _uow.SaveChangesAsync(cancellationToken);

            var dto = await repo.GetAll(a => !a.IsDeleted && a.Id == entity.Id)
                .Select(ContentDtoMapper.HelpArticleProjection)
                .FirstOrDefaultAsync(cancellationToken);
            return Result<HelpArticleDto>.Success(dto!, LocalizationKeys.HelpArticle.Updated);
        }
    }
}
