using MediatR;
using Microsoft.EntityFrameworkCore;
using Welco.Shared.Common.DTOs.Sales;
using Welco.Shared.Common.Interfaces;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Domain.Models;
using Welco.Shared.Localization;
using Welco.Shared.Results;
using Sales.Services.API.Features.Shared;

namespace Sales.Services.API.Features.ProductInquiries.Commands.RespondProductInquiry
{
    public class RespondProductInquiryCommandHandler : IRequestHandler<RespondProductInquiryCommand, Result<ProductInquiryDto>>
    {
        private readonly IUnitOfWork _uow;
        private readonly ICurrentUserService _cur;

        public RespondProductInquiryCommandHandler(IUnitOfWork uow, ICurrentUserService cur)
        {
            _uow = uow;
            _cur = cur;
        }

        public async Task<Result<ProductInquiryDto>> Handle(RespondProductInquiryCommand request, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(request.Response))
                return Result<ProductInquiryDto>.BadRequest(LocalizationKeys.ProductInquiry.ResponseRequired);

            var repo = _uow.GetRepository<ProductInquiry, Guid>();
            var inquiry = await repo.GetAll(x => !x.IsDeleted && x.Id == request.Id)
                .Include(x => x.Product)
                .FirstOrDefaultAsync(ct);

            if (inquiry == null)
                return Result<ProductInquiryDto>.NotFound(LocalizationKeys.ProductInquiry.NotFound);

            var caller = await BuyerScope.GetAsync(_uow, _cur, ct);
            if (caller.IsOrganizationUser && caller.CompanyId.HasValue)
            {
                var companyId = caller.CompanyId.Value;
                if (inquiry.Product == null || inquiry.Product.CompanyId != companyId)
                    return Result<ProductInquiryDto>.NotFound(LocalizationKeys.ProductInquiry.NotFound);
            }

            var curId = _cur.UserId != Guid.Empty ? _cur.UserId.ToString() : "System";
            inquiry.Response = request.Response.Trim();
            inquiry.RespondedAt = DateTime.UtcNow;
            inquiry.RespondedById = _cur.UserId != Guid.Empty ? _cur.UserId : null;
            inquiry.Status = ProductInquiryStatus.Responded;
            inquiry.MarkAsUpdated(curId);

            await _uow.SaveChangesAsync(ct);

            var dto = new ProductInquiryDto
            {
                Id = inquiry.Id,
                ProductId = inquiry.ProductId,
                ProductNameEn = inquiry.Product?.NameEn,
                ProductNameAr = inquiry.Product?.NameAr,
                ProductSku = inquiry.Product?.Sku,
                Name = inquiry.Name,
                Organization = inquiry.Organization,
                Message = inquiry.Message,
                Email = inquiry.Email,
                Status = inquiry.Status.ToString(),
                UserId = inquiry.UserId,
                Response = inquiry.Response,
                RespondedAt = inquiry.RespondedAt,
                RespondedById = inquiry.RespondedById,
                CreatedAt = inquiry.CreatedAt
            };

            return Result<ProductInquiryDto>.Success(dto, LocalizationKeys.ProductInquiry.Responded);
        }
    }
}
