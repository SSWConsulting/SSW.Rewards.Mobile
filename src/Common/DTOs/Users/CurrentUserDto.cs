namespace SSW.Rewards.Shared.DTOs.Users;

public class CurrentUserDto
{
    public int Id { get; set; }

    public required string Email { get; set; }

    public required string FullName { get; set; }

    public string? ProfilePic { get; set; }

    public int Points { get; set; }

    public int Balance { get; set; }

    public string? QRCode { get; set; }
    
    public bool IsStaff { get; set; }
    
    public int Rank { get; set; }
}
