using MediatR;
using Welco.Shared.Common.DTOs.UserManagement;
using Welco.Shared.Results;

namespace UserManamgent.Service.API.Features.Companies.Queries.GetMyCompany
{
    public class GetMyCompanyQuery : IRequest<Result<CompanyDto>>
    {
    }
}
