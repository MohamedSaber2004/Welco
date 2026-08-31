using MediatR;
using Welco.Shared.Results;
namespace UserManamgent.Service.API.Features.Companies.Commands.DeleteCompany
{
    public class DeleteCompanyCommand : IRequest<Result<string>> { public Guid Id { get; set; } }
}
