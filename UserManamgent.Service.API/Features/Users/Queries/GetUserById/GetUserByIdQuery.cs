using MediatR;
using Welco.Shared.Common.DTOs.UserManagement;
using Welco.Shared.Results;

namespace UserManamgent.Service.API.Features.Users.Queries.GetUserById
{
    public class GetUserByIdQuery : IRequest<Result<UserDetailsDto>>
    {
        public Guid Id { get; set; }
    }
}
