using MediatR;
using Welco.Shared.Common.DTOs.Sales;
using Welco.Shared.Results;
namespace Sales.Services.API.Features.RFQs.Queries.GetRFQById
{
    public class GetRFQByIdQuery : IRequest<Result<RFQDto>> { public Guid Id { get; set; } }
}
