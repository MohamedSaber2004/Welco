using MediatR;
using Welco.Shared.Results;
namespace UserManamgent.Service.API.Features.Companies.Commands.DeleteProviderWithCascade
{
    public class DeleteProviderWithCascadeCommand : IRequest<Result<string>> { public Guid Id { get; set; } }
}
