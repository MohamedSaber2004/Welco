using MediatR;
using Welco.Shared.Common.Interfaces;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Domain.Models;
using Welco.Shared.Localization;
using Welco.Shared.Results;

namespace UserManamgent.Service.API.Features.CompanyAddresses.Commands.DeleteCompanyAddress
{
    public class DeleteCompanyAddressCommandHandler : IRequestHandler<DeleteCompanyAddressCommand, Result<string>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public DeleteCompanyAddressCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<Result<string>> Handle(DeleteCompanyAddressCommand request, CancellationToken cancellationToken)
        {
            var repo = _unitOfWork.GetRepository<CompanyAddress, Guid>();
            var address = await repo.GetByIdAsync(request.Id, cancellationToken);
            if (address == null || address.IsDeleted)
                return Result<string>.NotFound(LocalizationKeys.UserAddress.AddressNotFound);

            var wasDefault = address.IsDefault;
            var currentUserId = _currentUserService.UserId != Guid.Empty
                ? _currentUserService.UserId.ToString()
                : "System";

            address.MarkAsDeleted(currentUserId);
            repo.Update(address);

            if (wasDefault)
            {
                var remaining = await repo.GetAllListAsync(a => a.CompanyId == address.CompanyId && a.Id != address.Id && !a.IsDeleted, cancellationToken);
                var next = remaining.OrderByDescending(a => a.CreatedAt).FirstOrDefault();
                if (next != null)
                {
                    next.IsDefault = true;
                    next.MarkAsUpdated(currentUserId);
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result<string>.Success(address.Id.ToString(), LocalizationKeys.UserAddress.AddressDeleted);
        }
    }
}
