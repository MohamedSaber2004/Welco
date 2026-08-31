using MediatR;
using Welco.Shared.Common.Interfaces;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Localization;
using Welco.Shared.Results;
using DocEntity = Welco.Shared.Domain.Models.Document;

namespace Content.Services.API.Features.Documents.Commands.DeleteDocument
{
    public class DeleteDocumentCommandHandler : IRequestHandler<DeleteDocumentCommand, Result<string>>
    {
        private readonly IUnitOfWork _uow;
        private readonly ICurrentUserService _currentUser;

        public DeleteDocumentCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
        {
            _uow = uow;
            _currentUser = currentUser;
        }

        public async Task<Result<string>> Handle(DeleteDocumentCommand request, CancellationToken cancellationToken)
        {
            var repo = _uow.GetRepository<DocEntity, Guid>();
            var doc = await repo.GetByIdAsync(request.Id, cancellationToken);

            if (doc == null || doc.IsDeleted)
                return Result<string>.NotFound(LocalizationKeys.Document.NotFound);

            var currentUserId = _currentUser.UserId != Guid.Empty ? _currentUser.UserId.ToString() : "System";
            doc.MarkAsDeleted(currentUserId);
            repo.Update(doc);
            await _uow.SaveChangesAsync(cancellationToken);

            return Result<string>.Success(doc.Id.ToString(), LocalizationKeys.Document.Deleted);
        }
    }
}
