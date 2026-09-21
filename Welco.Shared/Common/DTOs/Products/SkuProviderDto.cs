using Welco.Shared.Common.DTOs.UserManagement;

namespace Welco.Shared.Common.DTOs.Products
{
    /// <summary>
    /// One provider offering a given SKU, with that provider's listing.
    /// Mediator model: the same instrument can be supplied by many companies.
    /// </summary>
    public class SkuProviderDto
    {
        public CompanyDto? Company { get; set; }
        public ProductDto Listing { get; set; } = null!;
    }
}
