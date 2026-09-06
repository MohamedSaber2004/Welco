using MediatR;
using Welco.Shared.Common.Interfaces;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Domain.Models;
using Welco.Shared.Localization;
using Welco.Shared.Results;
namespace Content.Services.API.Features.OemInquiries.Commands.DeleteOemInquiry
{
    public class DeleteOemInquiryCommandHandler : IRequestHandler<DeleteOemInquiryCommand, Result<string>>
    {
        private readonly IUnitOfWork _uow;
        private readonly ICurrentUserService _currentUser;
        public DeleteOemInquiryCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser) { _uow = uow; _currentUser = currentUser; }
        public async Task<Result<string>> Handle(DeleteOemInquiryCommand request, CancellationToken cancellationToken)
        {
            var repo = _uow.GetRepository<OemInquiry, Guid>();
            var entity = await repo.GetByIdAsync(request.Id, cancellationToken);
            if (entity == null || entity.IsDeleted) return Result<string>.NotFound(LocalizationKeys.OemInquiry.NotFound);
            var currentUserId = _currentUser.UserId != Guid.Empty ? _currentUser.UserId.ToString() : "System";
            entity.MarkAsDeleted(currentUserId);
            repo.Update(entity);
            await _uow.SaveChangesAsync(cancellationToken);
            return Result<string>.Success(entity.Id.ToString(), LocalizationKeys.OemInquiry.Deleted);
        }
    }
}
