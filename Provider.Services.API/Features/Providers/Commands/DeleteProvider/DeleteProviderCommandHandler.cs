using MediatR;
using Welco.Shared.Common.Interfaces;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Domain.Models;
using Welco.Shared.Localization;
using Welco.Shared.Results;
using ProviderEntity = Welco.Shared.Domain.Models.Provider;

namespace Provider.Services.API.Features.Providers.Commands.DeleteProvider
{
    public class DeleteProviderCommandHandler : IRequestHandler<DeleteProviderCommand, Result<string>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public DeleteProviderCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<Result<string>> Handle(DeleteProviderCommand request, CancellationToken cancellationToken)
        {
            var providerRepo = _unitOfWork.GetRepository<ProviderEntity, Guid>();
            var provider = await providerRepo.GetByIdAsync(request.Id, cancellationToken);
            if (provider == null || provider.IsDeleted)
            {
                return Result<string>.NotFound(LocalizationKeys.Provider.NotFound);
            }

            var currentUserId = _currentUserService.UserId != Guid.Empty
                ? _currentUserService.UserId.ToString()
                : "System";

            provider.MarkAsDeleted(currentUserId);
            providerRepo.Update(provider);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<string>.Success(provider.Id.ToString(), LocalizationKeys.Provider.Deleted);
        }
    }
}
