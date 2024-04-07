namespace SSW.Rewards.Shared.DTOs.Users;

public class AdminDeleteProfileDto
{
    public int UserId { get; set; } = 0;

    public bool AdminConfirmed1 { get; set; } = false;

    public bool AdminConfirmed2 { get; set; } = false;
}
