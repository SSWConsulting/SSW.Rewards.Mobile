namespace SSW.Rewards.Application.Users.Commands.Common;

public class DeleteProfileEmail
{
    public string UserName { get; set; }

    public string UserEmail { get; set; }

    public string RewardsTeamEmail { get; set; }

    public DateTime RequestDate { get; set; }
}
