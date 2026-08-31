using MediatR;
using Welco.Shared.Common.DTOs.UserManagement;
using Welco.Shared.Results;
namespace UserManamgent.Service.API.Features.Companies.Queries.GetCompanyById
{
    public class GetCompanyByIdQuery : IRequest<Result<CompanyDto>> { public Guid Id { get; set; } }
}
