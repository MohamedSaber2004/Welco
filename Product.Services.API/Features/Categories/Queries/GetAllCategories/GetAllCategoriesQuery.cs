using MediatR;
using Welco.Shared.Common.DTOs.Products;
using Welco.Shared.Results;

namespace Product.Services.API.Features.Categories.Queries.GetAllCategories
{
    public class GetAllCategoriesQuery : IRequest<Result<List<CategoryDto>>>
    {
    }
}