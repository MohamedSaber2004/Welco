using MediatR;
using Welco.Shared.Results;

namespace Product.Services.API.Features.Products.Commands.DeleteProduct
{
    public class DeleteProductCommand : IRequest<Result<string>>
    {
        public Guid Id { get; set; }
    }
}
