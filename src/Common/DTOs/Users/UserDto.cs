using SSW.Rewards.Shared.DTOs.Roles;

namespace SSW.Rewards.Shared.DTOs.Users;

public class UserDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;
    
    public IEnumerable<RoleDto> Roles { get; set; } = Enumerable.Empty<RoleDto>();
}
