using MediatR;
using Welco.Shared.Common.DTOs.UserManagement;
using Welco.Shared.Enums;
using Welco.Shared.Results;

namespace UserManamgent.Service.API.Features.Users.Queries.GetUsers
{
    public class GetUsersQuery : IRequest<PaginatedResult<UserDto>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SearchTerm { get; set; }
        public UserType? UserType { get; set; }
        public bool? IsActive { get; set; }
    }
}
