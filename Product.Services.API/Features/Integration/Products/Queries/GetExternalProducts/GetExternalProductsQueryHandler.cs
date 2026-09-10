using MediatR;
using Microsoft.EntityFrameworkCore;
using Welco.Shared.Common.DTOs.Integration;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Results;
using ProductEntity = Welco.Shared.Domain.Models.Product;

namespace Product.Services.API.Features.Integration.Products.Queries.GetExternalProducts
{
    public class GetExternalProductsQueryHandler : IRequestHandler<GetExternalProductsQuery, Result<List<ExternalProductDto>>>
    {
        private readonly IUnitOfWork _uow;

        public GetExternalProductsQueryHandler(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<Result<List<ExternalProductDto>>> Handle(GetExternalProductsQuery request, CancellationToken ct)
        {
            var page = Math.Max(1, request.Page);
            var pageSize = Math.Clamp(request.PageSize, 1, 200);

            var productRepo = _uow.GetRepository<ProductEntity, Guid>();
            var products = await productRepo
                .GetAll(p => !p.IsDeleted)
                .OrderBy(p => p.NameEn)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new ExternalProductDto
                {
                    Id = p.Id,
                    NameEn = p.NameEn,
                    NameAr = p.NameAr,
                    Sku = p.Sku,
                    Slug = p.Slug,
                    Description = p.Description,
                    Price = p.Price,
                    Stock = p.Stock,
                    ImageName = p.ImageName,
                    CategoryId = p.CategoryId
                })
                .AsNoTracking()
                .ToListAsync(ct);

            return Result<List<ExternalProductDto>>.Success(products);
        }
    }
}
