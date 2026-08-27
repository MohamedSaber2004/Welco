using MediatR;
using Welco.Shared.Results;

namespace UserManamgent.Service.API.Features.Users.Commands.DeleteUser
{
    public class DeleteUserCommand : IRequest<Result<string>>
    {
        public Guid Id { get; set; }
    }
}
