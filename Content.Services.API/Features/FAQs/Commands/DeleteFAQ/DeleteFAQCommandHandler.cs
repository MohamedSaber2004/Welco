using MediatR;
using Welco.Shared.Common.Interfaces;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Localization;
using Welco.Shared.Results;
using FAQEntity = Welco.Shared.Domain.Models.FAQItem;

namespace Content.Services.API.Features.FAQs.Commands.DeleteFAQ
{
    public class DeleteFAQCommandHandler : IRequestHandler<DeleteFAQCommand, Result<string>>
    {
        private readonly IUnitOfWork _uow;
        private readonly ICurrentUserService _currentUser;
        public DeleteFAQCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser) { _uow = uow; _currentUser = currentUser; }

        public async Task<Result<string>> Handle(DeleteFAQCommand request, CancellationToken cancellationToken)
        {
            var repo = _uow.GetRepository<FAQEntity, Guid>();
            var entity = await repo.GetByIdAsync(request.Id, cancellationToken);
            if (entity == null || entity.IsDeleted) return Result<string>.NotFound(LocalizationKeys.FAQ.NotFound);
            var currentUserId = _currentUser.UserId != Guid.Empty ? _currentUser.UserId.ToString() : "System";
            entity.MarkAsDeleted(currentUserId);
            repo.Update(entity);
            await _uow.SaveChangesAsync(cancellationToken);
            return Result<string>.Success(entity.Id.ToString(), LocalizationKeys.FAQ.Deleted);
        }
    }
}
