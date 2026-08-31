using MediatR;
using Welco.Shared.Common.DTOs.UserManagement;
using Welco.Shared.Results;

namespace UserManamgent.Service.API.Features.Companies.Queries.GetCompanies
{
    public class GetCompaniesQuery : IRequest<PaginatedResult<CompanyDto>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SearchTerm { get; set; }
        public bool? IsActive { get; set; }
    }
}
