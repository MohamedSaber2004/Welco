using MediatR;
using Welco.Shared.Results;
namespace Content.Services.API.Features.OemInquiries.Commands.DeleteOemInquiry
{
    public class DeleteOemInquiryCommand : IRequest<Result<string>> { public Guid Id { get; set; } }
}
