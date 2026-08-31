using MediatR;
using Welco.Shared.Results;
namespace Sales.Services.API.Features.RFQs.Commands.UpdateRFQStatus
{
    public class UpdateRFQStatusCommand : IRequest<Result<string>> { public Guid Id { get; set; } public string Status { get; set; } = string.Empty; }
}
