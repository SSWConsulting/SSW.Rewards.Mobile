namespace SSW.Rewards.Mobile.Messages;

public class UserDetailsUpdatedMessage
{
    public string Name { get; set; }

    public string Email { get; set; }

    public string ProfilePic { get; set; }

    public bool IsStaff { get; set; }
}
