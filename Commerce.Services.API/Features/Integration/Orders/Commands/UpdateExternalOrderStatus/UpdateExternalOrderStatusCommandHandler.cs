using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Welco.Shared.Common.Repositories.Interfaces.Base;
using Welco.Shared.Domain.Models;
using Welco.Shared.Results;
using OrderEntity = Welco.Shared.Domain.Models.Order;

namespace Commerce.Services.API.Features.Integration.Orders.Commands.UpdateExternalOrderStatus
{
    public class UpdateExternalOrderStatusCommandHandler : IRequestHandler<UpdateExternalOrderStatusCommand, Result<bool>>
    {
        private readonly IUnitOfWork _uow;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<UpdateExternalOrderStatusCommandHandler> _logger;

        public UpdateExternalOrderStatusCommandHandler(
            IUnitOfWork uow,
            IHttpContextAccessor httpContextAccessor,
            ILogger<UpdateExternalOrderStatusCommandHandler> logger)
        {
            _uow = uow;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<Result<bool>> Handle(UpdateExternalOrderStatusCommand request, CancellationToken ct)
        {
            _logger.LogInformation("[Integration MediatR] UpdateExternalOrderStatus WelcoOrderId={WelcoOrderId} Status={Status}",
                request.Id, request.Status);

            if (!Enum.TryParse<OrderStatus>(request.Status, ignoreCase: true, out var newStatus))
            {
                return Result<bool>.BadRequest($"Invalid status value: {request.Status}");
            }

            var orderRepo = _uow.GetRepository<OrderEntity, Guid>();
            var order = await orderRepo.GetAll(o => !o.IsDeleted && o.Id == request.Id).FirstOrDefaultAsync(ct);

            if (order == null)
            {
                return Result<bool>.NotFound("Order not found.");
            }

            order.Status = newStatus;
            order.MarkAsUpdated("integration-service");
            await _uow.SaveChangesAsync(ct);

            _logger.LogInformation("[Integration MediatR] Order status updated WelcoOrderId={WelcoOrderId} NewStatus={NewStatus}",
                request.Id, newStatus);

            return Result<bool>.Success(true);
        }
    }
}
