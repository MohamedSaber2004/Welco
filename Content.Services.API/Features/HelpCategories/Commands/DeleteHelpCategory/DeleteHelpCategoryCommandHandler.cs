using MediatR;
using Welco.Shared.Common.Interfaces;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Localization;
using Welco.Shared.Results;
using HelpCategoryEntity = Welco.Shared.Domain.Models.HelpCategory;

namespace Content.Services.API.Features.HelpCategories.Commands.DeleteHelpCategory
{
    public class DeleteHelpCategoryCommandHandler : IRequestHandler<DeleteHelpCategoryCommand, Result<string>>
    {
        private readonly IUnitOfWork _uow;
        private readonly ICurrentUserService _currentUser;

        public DeleteHelpCategoryCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
        {
            _uow = uow;
            _currentUser = currentUser;
        }

        public async Task<Result<string>> Handle(DeleteHelpCategoryCommand request, CancellationToken cancellationToken)
        {
            var repo = _uow.GetRepository<HelpCategoryEntity, Guid>();
            var entity = await repo.GetByIdAsync(request.Id, cancellationToken);
            if (entity == null || entity.IsDeleted)
                return Result<string>.NotFound(LocalizationKeys.HelpCategory.NotFound);

            var currentUserId = _currentUser.UserId != Guid.Empty ? _currentUser.UserId.ToString() : "System";
            entity.MarkAsDeleted(currentUserId);
            repo.Update(entity);
            await _uow.SaveChangesAsync(cancellationToken);
            return Result<string>.Success(entity.Id.ToString(), LocalizationKeys.HelpCategory.Deleted);
        }
    }
}
