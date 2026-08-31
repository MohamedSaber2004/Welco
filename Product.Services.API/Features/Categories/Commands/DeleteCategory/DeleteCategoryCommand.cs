using MediatR;
using Welco.Shared.Results;

namespace Product.Services.API.Features.Categories.Commands.DeleteCategory
{
    public class DeleteCategoryCommand : IRequest<Result<string>>
    {
        public Guid Id { get; set; }
    }
}
