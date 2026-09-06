using MediatR;
using Welco.Shared.Common.DTOs.Content;
using Welco.Shared.Domain.Models;
using Welco.Shared.Localization;
using Welco.Shared.Persistance;
using Welco.Shared.Results;
namespace Content.Services.API.Features.OemInquiries.Commands.CreateOemInquiry
{
    public class CreateOemInquiryCommandHandler : IRequestHandler<CreateOemInquiryCommand, Result<OemInquiryDto>>
    {
        private readonly WelcoDbContext _db;
        public CreateOemInquiryCommandHandler(WelcoDbContext db) => _db = db;
        public async Task<Result<OemInquiryDto>> Handle(CreateOemInquiryCommand request, CancellationToken ct)
        {
            var entity = new OemInquiry
            {
                Id = Guid.NewGuid(),
                FullName = request.FullName.Trim(),
                Email = request.Email.Trim(),
                CompanyName = request.CompanyName.Trim(),
                ServiceType = request.ServiceType.Trim(),
                Message = request.Message.Trim(),
            };
            entity.MarkAsCreated(request.Email.Trim());
            _db.OemInquiries.Add(entity);
            await _db.SaveChangesAsync(ct);
            var dto = new OemInquiryDto { Id = entity.Id, FullName = entity.FullName, Email = entity.Email, CompanyName = entity.CompanyName, ServiceType = entity.ServiceType, Message = entity.Message, CreatedAt = entity.CreatedAt };
            return Result<OemInquiryDto>.Success(dto, LocalizationKeys.OemInquiry.Created, 201);
        }
    }
}
