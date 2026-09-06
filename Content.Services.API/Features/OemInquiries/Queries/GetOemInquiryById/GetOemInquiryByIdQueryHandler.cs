using MediatR;
using Welco.Shared.Common.DTOs.Content;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Domain.Models;
using Welco.Shared.Localization;
using Welco.Shared.Results;
namespace Content.Services.API.Features.OemInquiries.Queries.GetOemInquiryById
{
    public class GetOemInquiryByIdQueryHandler : IRequestHandler<GetOemInquiryByIdQuery, Result<OemInquiryDto>>
    {
        private readonly IUnitOfWork _uow;
        public GetOemInquiryByIdQueryHandler(IUnitOfWork uow) => _uow = uow;
        public async Task<Result<OemInquiryDto>> Handle(GetOemInquiryByIdQuery r, CancellationToken ct)
        {
            var repo = _uow.GetRepository<OemInquiry, Guid>();
            var x = await repo.GetByIdAsync(r.Id, ct);
            if (x == null || x.IsDeleted) return Result<OemInquiryDto>.NotFound(LocalizationKeys.OemInquiry.NotFound);
            return Result<OemInquiryDto>.Success(new OemInquiryDto { Id = x.Id, FullName = x.FullName, Email = x.Email, CompanyName = x.CompanyName, ServiceType = x.ServiceType, Message = x.Message, CreatedAt = x.CreatedAt }, LocalizationKeys.OemInquiry.Fetched);
        }
    }
}
