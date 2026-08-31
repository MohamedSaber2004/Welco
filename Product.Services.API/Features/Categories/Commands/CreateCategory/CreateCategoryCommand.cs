using MediatR;
using Welco.Shared.Common.DTOs.Products;
using Welco.Shared.Results;

namespace Product.Services.API.Features.Categories.Commands.CreateCategory
{
    public class CreateCategoryCommand : IRequest<Result<CategoryDto>>
    {
        public string NameEn { get; set; } = string.Empty;
        public string NameAr { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ImageName { get; set; }
        public Guid? ParentCategoryId { get; set; }
    }
}
