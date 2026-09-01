using MediatR;
using Welco.Shared.Common.Interfaces;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Localization;
using Welco.Shared.Results;
using HelpArticleEntity = Welco.Shared.Domain.Models.HelpArticle;

namespace Content.Services.API.Features.HelpArticles.Commands.DeleteHelpArticle
{
    public class DeleteHelpArticleCommandHandler : IRequestHandler<DeleteHelpArticleCommand, Result<string>>
    {
        private readonly IUnitOfWork _uow;
        private readonly ICurrentUserService _currentUser;
        public DeleteHelpArticleCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser) { _uow = uow; _currentUser = currentUser; }

        public async Task<Result<string>> Handle(DeleteHelpArticleCommand request, CancellationToken cancellationToken)
        {
            var repo = _uow.GetRepository<HelpArticleEntity, Guid>();
            var entity = await repo.GetByIdAsync(request.Id, cancellationToken);
            if (entity == null || entity.IsDeleted) return Result<string>.NotFound(LocalizationKeys.HelpArticle.NotFound);
            var currentUserId = _currentUser.UserId != Guid.Empty ? _currentUser.UserId.ToString() : "System";
            entity.MarkAsDeleted(currentUserId);
            repo.Update(entity);
            await _uow.SaveChangesAsync(cancellationToken);
            return Result<string>.Success(entity.Id.ToString(), LocalizationKeys.HelpArticle.Deleted);
        }
    }
}
