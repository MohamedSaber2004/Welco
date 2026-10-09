using MediatR;
using Welco.Shared.Common.DTOs.Sales;
using Welco.Shared.Common.Interfaces;
using Welco.Shared.Domain.Models;
using Welco.Shared.Localization;
using Welco.Shared.Persistance;
using Welco.Shared.Results;

namespace Sales.Services.API.Features.ProductInquiries.Commands.CreateProductInquiry
{
    public class CreateProductInquiryCommandHandler : IRequestHandler<CreateProductInquiryCommand, Result<ProductInquiryDto>>
    {
        private readonly WelcoDbContext _db;
        private readonly ICurrentUserService _cur;

        public CreateProductInquiryCommandHandler(WelcoDbContext db, ICurrentUserService cur)
        {
            _db = db;
            _cur = cur;
        }

        public async Task<Result<ProductInquiryDto>> Handle(CreateProductInquiryCommand request, CancellationToken ct)
        {
            var product = await _db.Products.FindAsync(new object[] { request.ProductId }, ct);
            if (product == null || product.IsDeleted)
                return Result<ProductInquiryDto>.NotFound(LocalizationKeys.Product.NotFound);

            var email = string.IsNullOrWhiteSpace(request.Email)
                ? (!string.IsNullOrWhiteSpace(_cur.Email) ? _cur.Email : null)
                : request.Email.Trim();

            var entity = new ProductInquiry
            {
                Id = Guid.NewGuid(),
                ProductId = request.ProductId,
                Name = request.Name.Trim(),
                Organization = request.Organization.Trim(),
                Message = request.Message.Trim(),
                Email = email,
                UserId = _cur.UserId != Guid.Empty ? _cur.UserId : null,
                Status = ProductInquiryStatus.Pending
            };

            var creator = _cur.UserId != Guid.Empty ? _cur.UserId.ToString() : (email ?? "Anonymous");
            entity.MarkAsCreated(creator);

            _db.ProductInquiries.Add(entity);
            await _db.SaveChangesAsync(ct);

            var dto = new ProductInquiryDto
            {
                Id = entity.Id,
                ProductId = entity.ProductId,
                ProductNameEn = product.NameEn,
                ProductNameAr = product.NameAr,
                ProductSku = product.Sku,
                Name = entity.Name,
                Organization = entity.Organization,
                Message = entity.Message,
                Email = entity.Email,
                Status = entity.Status.ToString(),
                UserId = entity.UserId,
                CreatedAt = entity.CreatedAt
            };
            return Result<ProductInquiryDto>.Success(dto, LocalizationKeys.ProductInquiry.Created, 201);
        }
    }
}
