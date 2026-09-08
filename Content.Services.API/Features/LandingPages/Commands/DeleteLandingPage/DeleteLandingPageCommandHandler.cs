using MediatR;
using Welco.Shared.Common.Interfaces;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Localization;
using Welco.Shared.Results;
using LandingPageEntity = Welco.Shared.Domain.Models.LandingPage;

namespace Content.Services.API.Features.LandingPages.Commands.DeleteLandingPage
{
    public class DeleteLandingPageCommandHandler : IRequestHandler<DeleteLandingPageCommand, Result<string>>
    {
        private readonly IUnitOfWork _uow;
        private readonly ICurrentUserService _currentUser;

        public DeleteLandingPageCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
        {
            _uow = uow;
            _currentUser = currentUser;
        }

        public async Task<Result<string>> Handle(DeleteLandingPageCommand request, CancellationToken cancellationToken)
        {
            var repo = _uow.GetRepository<LandingPageEntity, Guid>();
            var entity = await repo.GetByIdAsync(request.Id, cancellationToken);

            if (entity == null || entity.IsDeleted)
                return Result<string>.NotFound(LocalizationKeys.LandingPage.NotFound);

            var currentUserId = _currentUser.UserId != Guid.Empty ? _currentUser.UserId.ToString() : "System";
            entity.MarkAsDeleted(currentUserId);
            repo.Update(entity);
            await _uow.SaveChangesAsync(cancellationToken);

            return Result<string>.Success(entity.Id.ToString(), LocalizationKeys.LandingPage.Deleted);
        }
    }
}
