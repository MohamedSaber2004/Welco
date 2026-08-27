using MediatR;
using Welco.Shared.Common.Interfaces;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Domain.Models;
using Welco.Shared.Localization;
using Welco.Shared.Results;

namespace UserManamgent.Service.API.Features.Cities.Commands.DeleteCity
{
    public class DeleteCityCommandHandler : IRequestHandler<DeleteCityCommand, Result<string>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public DeleteCityCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<Result<string>> Handle(DeleteCityCommand request, CancellationToken cancellationToken)
        {
            var cityRepo = _unitOfWork.GetRepository<City, Guid>();
            var city = await cityRepo.GetByIdAsync(request.Id, cancellationToken);
            if (city == null || city.IsDeleted)
            {
                return Result<string>.NotFound(LocalizationKeys.City.NotFound);
            }

            var currentUserId = _currentUserService.UserId != Guid.Empty
                ? _currentUserService.UserId.ToString()
                : "System";

            city.MarkAsDeleted(currentUserId);
            cityRepo.Update(city);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<string>.Success(city.Id.ToString(), LocalizationKeys.City.Deleted);
        }
    }
}
