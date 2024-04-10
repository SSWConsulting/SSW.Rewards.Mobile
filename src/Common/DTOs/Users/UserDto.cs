namespace SSW.Rewards.Shared.DTOs.Users;

public class UserDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;
    
    public IEnumerable<UserRoleDto> Roles { get; set; } = Enumerable.Empty<UserRoleDto>();
}
