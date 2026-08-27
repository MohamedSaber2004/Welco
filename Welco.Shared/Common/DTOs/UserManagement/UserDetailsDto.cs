using Welco.Shared.Enums;

namespace Welco.Shared.Common.DTOs.UserManagement
{
    public class UserDetailsDto : UserDto
    {
        public IReadOnlyList<UserAddressDto> Addresses { get; set; } = new List<UserAddressDto>();
    }
}
