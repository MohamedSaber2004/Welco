using Content.Services.API.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Welco.Shared.Common.DTOs.Content;
using Welco.Shared.Common.Interfaces;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Localization;
using Welco.Shared.Results;
using HelpCategoryEntity = Welco.Shared.Domain.Models.HelpCategory;

namespace Content.Services.API.Features.HelpCategories.Commands.CreateHelpCategory
{
    public class CreateHelpCategoryCommandHandler : IRequestHandler<CreateHelpCategoryCommand, Result<HelpCategoryDto>>
    {
        private readonly IUnitOfWork _uow;
        private readonly ICurrentUserService _currentUser;

        public CreateHelpCategoryCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser)
        {
            _uow = uow;
            _currentUser = currentUser;
        }

        public async Task<Result<HelpCategoryDto>> Handle(CreateHelpCategoryCommand request, CancellationToken cancellationToken)
        {
            var repo = _uow.GetRepository<HelpCategoryEntity, Guid>();
            var exists = await repo.ExistsAsync(c => !c.IsDeleted && c.Name.ToLower() == request.Name.Trim().ToLower(), cancellationToken);
            if (exists)
                return Result<HelpCategoryDto>.Conflict(LocalizationKeys.HelpCategory.AlreadyExists);

            var currentUserId = _currentUser.UserId != Guid.Empty ? _currentUser.UserId.ToString() : "System";
            var entity = new HelpCategoryEntity
            {
                Id = Guid.NewGuid(),
                Name = request.Name.Trim(),
                Icon = request.Icon?.Trim()
            };
            entity.MarkAsCreated(currentUserId);
            await repo.AddAsync(entity, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);

            var dto = await repo.GetAll(c => !c.IsDeleted && c.Id == entity.Id)
                .Select(ContentDtoMapper.HelpCategoryProjection)
                .FirstOrDefaultAsync(cancellationToken);

            return Result<HelpCategoryDto>.Created(dto!, LocalizationKeys.HelpCategory.Created);
        }
    }
}
