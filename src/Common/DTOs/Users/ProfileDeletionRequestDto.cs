namespace SSW.Rewards.Shared.DTOs.Users;

public class ProfileDeletionRequestDto
{
    public int UserId { get; set; }

    public string UserName { get; set; }

    public string Requested { get; set; }
}

public class ProfileDeletionRequestsVieWModel
{
    public List<ProfileDeletionRequestDto> Requests { get; set; } = new();
}