namespace SSW.Rewards.Shared.DTOs.Users;

public class UsersViewModel
{
    public IEnumerable<UserDto> Users { get; set; } = Enumerable.Empty<UserDto>();
}