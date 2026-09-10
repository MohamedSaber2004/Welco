using MediatR;
using Microsoft.EntityFrameworkCore;
using Welco.Shared.Common.DTOs.Integration;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Results;
using ProductEntity = Welco.Shared.Domain.Models.Product;

namespace Product.Services.API.Features.Integration.Products.Queries.GetExternalProductById
{
    public class GetExternalProductByIdQueryHandler : IRequestHandler<GetExternalProductByIdQuery, Result<ExternalProductDto>>
    {
        private readonly IUnitOfWork _uow;

        public GetExternalProductByIdQueryHandler(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<Result<ExternalProductDto>> Handle(GetExternalProductByIdQuery request, CancellationToken ct)
        {
            var productRepo = _uow.GetRepository<ProductEntity, Guid>();
            var product = await productRepo
                .GetAll(p => !p.IsDeleted && p.Id == request.Id)
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
                .FirstOrDefaultAsync(ct);

            if (product == null)
            {
                return Result<ExternalProductDto>.NotFound("Product not found.");
            }

            return Result<ExternalProductDto>.Success(product);
        }
    }
}
