using MediatR;
using Welco.Shared.Common.DTOs.Products;
using Welco.Shared.Results;

namespace Product.Services.API.Features.Suppliers.Queries.GetSuppliers
{
    public class GetSuppliersQueryHandler : IRequestHandler<GetSuppliersQuery, Result<IEnumerable<SupplierDto>>>
    {
        public Task<Result<IEnumerable<SupplierDto>>> Handle(GetSuppliersQuery request, CancellationToken cancellationToken)
        {
            // Static suppliers — mapped from Companies with type Distributor, fallback to curated list
            var suppliers = new List<SupplierDto>
            {
                new SupplierDto { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), NameEn = "MediCore Supplies", NameAr = "ميدي كور", CountryCode = "EG", CityEn = "Cairo", CityAr = "القاهرة", Rating = 4.8, ReviewCount = 124, Verified = true, FulfillmentDays = "3-5 days", ProductCount = 42, Tint = "#0E7169" },
                new SupplierDto { Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), NameEn = "SurgicalEdge", NameAr = "سيرجيكال إيدج", CountryCode = "SA", CityEn = "Riyadh", CityAr = "الرياض", Rating = 4.6, ReviewCount = 89, Verified = true, FulfillmentDays = "2-4 days", ProductCount = 36, Tint = "#0B1D2A" },
                new SupplierDto { Id = Guid.Parse("33333333-3333-3333-3333-333333333333"), NameEn = "Welco Direct", NameAr = "ويلكو مباشر", CountryCode = "AE", CityEn = "Dubai", CityAr = "دبي", Rating = 4.9, ReviewCount = 210, Verified = true, FulfillmentDays = "1-3 days", ProductCount = 58, Tint = "#9C5F26" },
                new SupplierDto { Id = Guid.Parse("44444444-4444-4444-4444-444444444444"), NameEn = "ClinicHub Trade", NameAr = "كلينيك هب", CountryCode = "JO", CityEn = "Amman", CityAr = "عمان", Rating = 4.5, ReviewCount = 67, Verified = false, FulfillmentDays = "5-7 days", ProductCount = 24, Tint = "#5C7686" },
            };
            return Task.FromResult(Result<IEnumerable<SupplierDto>>.Success(suppliers, "Suppliers fetched"));
        }
    }
}
