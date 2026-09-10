using MediatR;
using Microsoft.EntityFrameworkCore;
using Welco.Shared.Common.DTOs.Integration;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Results;
using CategoryEntity = Welco.Shared.Domain.Models.Category;

namespace Product.Services.API.Features.Integration.Categories.Queries.GetExternalCategoryById
{
    public class GetExternalCategoryByIdQuery : IRequest<Result<ExternalCategoryDto>>
    {
        public Guid Id { get; set; }
    }

    public class GetExternalCategoryByIdQueryHandler : IRequestHandler<GetExternalCategoryByIdQuery, Result<ExternalCategoryDto>>
    {
        private readonly IUnitOfWork _uow;

        public GetExternalCategoryByIdQueryHandler(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<Result<ExternalCategoryDto>> Handle(GetExternalCategoryByIdQuery request, CancellationToken ct)
        {
            var categoryRepo = _uow.GetRepository<CategoryEntity, Guid>();
            var category = await categoryRepo
                .GetAll(c => !c.IsDeleted && c.Id == request.Id)
                .Select(c => new ExternalCategoryDto
                {
                    Id = c.Id,
                    NameEn = c.NameEn,
                    NameAr = c.NameAr,
                    Description = c.Description,
                    ImageName = c.ImageName,
                    ParentCategoryId = c.ParentCategoryId
                })
                .AsNoTracking()
                .FirstOrDefaultAsync(ct);

            if (category == null)
            {
                return Result<ExternalCategoryDto>.NotFound("Category not found.");
            }

            return Result<ExternalCategoryDto>.Success(category);
        }
    }
}
