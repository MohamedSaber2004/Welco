using MediatR;
using Welco.Shared.Common.DTOs.UserManagement;
using Welco.Shared.Results;

namespace Product.Services.API.Features.Categories.Queries.GetCategoryProviders
{
    /// <summary>
    /// Providers offering at least one active product in the category
    /// (directly or via a child category). Mediator model: buyers browse
    /// who supplies a category, then drill into each provider catalog.
    /// </summary>
    public class GetCategoryProvidersQuery : IRequest<PaginatedResult<CompanyDto>>
    {
        public Guid CategoryId { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
