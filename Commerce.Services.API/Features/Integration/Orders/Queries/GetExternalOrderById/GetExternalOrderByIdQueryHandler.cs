using MediatR;
using Microsoft.EntityFrameworkCore;
using Welco.Shared.Common.DTOs.Integration;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Results;
using OrderEntity = Welco.Shared.Domain.Models.Order;

namespace Commerce.Services.API.Features.Integration.Orders.Queries.GetExternalOrderById
{
    public class GetExternalOrderByIdQueryHandler : IRequestHandler<GetExternalOrderByIdQuery, Result<ExternalOrderResponse>>
    {
        private readonly IUnitOfWork _uow;

        public GetExternalOrderByIdQueryHandler(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<Result<ExternalOrderResponse>> Handle(GetExternalOrderByIdQuery request, CancellationToken ct)
        {
            var orderRepo = _uow.GetRepository<OrderEntity, Guid>();
            var order = await orderRepo.GetAll(o => !o.IsDeleted && o.Id == request.Id)
                .FirstOrDefaultAsync(ct);

            if (order == null)
            {
                return Result<ExternalOrderResponse>.NotFound("Order not found.");
            }

            return Result<ExternalOrderResponse>.Success(new ExternalOrderResponse
            {
                WelcoOrderId = order.Id,
                WelcoOrderNumber = order.OrderNumber,
                Status = order.Status.ToString()
            });
        }
    }
}
