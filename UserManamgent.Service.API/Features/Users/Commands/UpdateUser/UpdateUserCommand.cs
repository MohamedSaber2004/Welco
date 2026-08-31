using MediatR;
using Welco.Shared.Common.DTOs.UserManagement;
using Welco.Shared.Enums;
using Welco.Shared.Results;

namespace UserManamgent.Service.API.Features.Users.Commands.UpdateUser
{
    public class UpdateUserCommand : IRequest<Result<UserDto>>
    {
        public Guid Id { get; set; }
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? ProfilePictureName { get; set; }
        public UserType? UserType { get; set; }
        public Guid? CompanyId { get; set; }
        public bool? IsActive { get; set; }
    }
}
