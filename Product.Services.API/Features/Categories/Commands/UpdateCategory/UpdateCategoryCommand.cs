using MediatR;
using Welco.Shared.Common.DTOs.Products;
using Welco.Shared.Results;

namespace Product.Services.API.Features.Categories.Commands.UpdateCategory
{
    public class UpdateCategoryCommand : IRequest<Result<CategoryDto>>
    {
        public Guid Id { get; set; }
        public string NameEn { get; set; } = string.Empty;
        public string NameAr { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ImageName { get; set; }
        public Guid? ParentCategoryId { get; set; }
        public bool? IsActive { get; set; }
    }
}
