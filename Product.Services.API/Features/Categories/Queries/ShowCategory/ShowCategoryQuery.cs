using MediatR;
using Welco.Shared.Common.DTOs.Products;
using Welco.Shared.Results;

namespace Product.Services.API.Features.Categories.Queries.ShowCategory
{
    public class ShowCategoryQuery : IRequest<Result<CategoryDto>>
    {
        public Guid Id { get; set; }
    }
}
