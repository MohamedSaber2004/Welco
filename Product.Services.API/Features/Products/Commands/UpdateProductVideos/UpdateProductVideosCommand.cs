using MediatR;
using Welco.Shared.Common.DTOs.Products;
using Welco.Shared.Results;

namespace Product.Services.API.Features.Products.Commands.UpdateProductVideos
{
    public class UpdateProductVideosCommand : IRequest<Result<IReadOnlyList<ProductMediaDto>>>
    {
        public Guid ProductId { get; set; }
        public List<UpdateProductVideoItemDto> Videos { get; set; } = new();
    }

    public class UpdateProductVideoItemDto
    {
        public string Url { get; set; } = string.Empty;
        public int SortOrder { get; set; }
    }
}
