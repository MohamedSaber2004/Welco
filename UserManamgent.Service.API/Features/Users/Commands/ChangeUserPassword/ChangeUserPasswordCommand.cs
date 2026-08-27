using MediatR;
using Welco.Shared.Results;

namespace UserManamgent.Service.API.Features.Users.Commands.ChangeUserPassword
{
    public class ChangeUserPasswordCommand : IRequest<Result<string>>
    {
        public Guid Id { get; set; }
        public string NewPassword { get; set; } = string.Empty;
    }
}
