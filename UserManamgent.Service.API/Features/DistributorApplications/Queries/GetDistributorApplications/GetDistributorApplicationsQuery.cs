using MediatR;
using Welco.Shared.Common.DTOs.UserManagement;
using Welco.Shared.Domain.Models;
using Welco.Shared.Results;

namespace UserManamgent.Service.API.Features.DistributorApplications.Queries.GetDistributorApplications
{
    public class GetDistributorApplicationsQuery : IRequest<PaginatedResult<DistributorApplicationDto>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SearchTerm { get; set; }
        public DistributorApplicationStatus? Status { get; set; }
    }
}
