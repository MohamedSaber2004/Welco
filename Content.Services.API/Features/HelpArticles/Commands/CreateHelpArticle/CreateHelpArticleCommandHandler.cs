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

namespace Content.Services.API.Features.HelpArticles.Commands.CreateHelpArticle
{
    public class CreateHelpArticleCommandHandler : IRequestHandler<CreateHelpArticleCommand, Result<HelpArticleDto>>
    {
        private readonly IUnitOfWork _uow;
        private readonly ICurrentUserService _currentUser;
        public CreateHelpArticleCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser) { _uow = uow; _currentUser = currentUser; }

        public async Task<Result<HelpArticleDto>> Handle(CreateHelpArticleCommand request, CancellationToken cancellationToken)
        {
            var catRepo = _uow.GetRepository<HelpCategoryEntity, Guid>();
            var catExists = await catRepo.ExistsAsync(c => !c.IsDeleted && c.Id == request.CategoryId, cancellationToken);
            if (!catExists) return Result<HelpArticleDto>.NotFound(LocalizationKeys.HelpCategory.NotFound);

            var repo = _uow.GetRepository<HelpArticleEntity, Guid>();
            var slug = request.Slug.Trim().ToLowerInvariant();
            var slugExists = await repo.ExistsAsync(a => !a.IsDeleted && a.Slug.ToLower() == slug, cancellationToken);
            if (slugExists) return Result<HelpArticleDto>.Conflict(LocalizationKeys.HelpArticle.SlugAlreadyExists);

            var currentUserId = _currentUser.UserId != Guid.Empty ? _currentUser.UserId.ToString() : "System";
            var entity = new HelpArticleEntity
            {
                Id = Guid.NewGuid(),
                CategoryId = request.CategoryId,
                Title = request.Title.Trim(),
                Body = request.Body.Trim(),
                Slug = slug
            };
            entity.MarkAsCreated(currentUserId);
            await repo.AddAsync(entity, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);

            var dto = await repo.GetAll(a => !a.IsDeleted && a.Id == entity.Id)
                .Select(ContentDtoMapper.HelpArticleProjection)
                .FirstOrDefaultAsync(cancellationToken);
            return Result<HelpArticleDto>.Created(dto!, LocalizationKeys.HelpArticle.Created);
        }
    }
}
