using Content.Services.API.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Welco.Shared.Common.DTOs.Content;
using Welco.Shared.Common.Interfaces;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Localization;
using Welco.Shared.Results;
using LandingPageEntity = Welco.Shared.Domain.Models.LandingPage;

namespace Content.Services.API.Features.LandingPages.Commands.UpdateLandingPage
{
    public class UpdateLandingPageCommandHandler : IRequestHandler<UpdateLandingPageCommand, Result<LandingPageDto>>
    {
        private readonly IUnitOfWork _uow;
        private readonly ICurrentUserService _currentUser;

        public UpdateLandingPageCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
        {
            _uow = uow;
            _currentUser = currentUser;
        }

        public async Task<Result<LandingPageDto>> Handle(UpdateLandingPageCommand request, CancellationToken cancellationToken)
        {
            var repo = _uow.GetRepository<LandingPageEntity, Guid>();
            var entity = await repo.GetByIdAsync(request.Id, cancellationToken);

            if (entity == null || entity.IsDeleted)
                return Result<LandingPageDto>.NotFound(LocalizationKeys.LandingPage.NotFound);

            var slug = request.Slug.Trim().ToLowerInvariant();
            if (!string.Equals(entity.Slug, slug, StringComparison.OrdinalIgnoreCase))
            {
                var slugExists = await repo.ExistsAsync(x => !x.IsDeleted && x.Id != request.Id && x.Slug.ToLower() == slug, cancellationToken);
                if (slugExists)
                    return Result<LandingPageDto>.Conflict(LocalizationKeys.LandingPage.SlugAlreadyExists);
            }

            var currentUserId = _currentUser.UserId != Guid.Empty ? _currentUser.UserId.ToString() : "System";

            entity.Type = request.Type.Trim();
            entity.Slug = slug;
            entity.HeroTitle = request.HeroTitle.Trim();
            entity.HeroBody = request.HeroBody;
            entity.ContentBlock = request.ContentBlock;
            entity.MarkAsUpdated(currentUserId);

            if (request.IsActive.HasValue)
                entity.SetActiveState(request.IsActive.Value, currentUserId);

            repo.Update(entity);
            await _uow.SaveChangesAsync(cancellationToken);

            var dto = await repo.GetAll(x => !x.IsDeleted && x.Id == entity.Id)
                .Select(ContentDtoMapper.LandingPageProjection)
                .FirstOrDefaultAsync(cancellationToken);

            return Result<LandingPageDto>.Success(dto!, LocalizationKeys.LandingPage.Updated);
        }
    }
}
