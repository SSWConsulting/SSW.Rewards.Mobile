namespace SSW.Rewards.Application.Common.Interfaces
{
    public interface ICurrentUserService
    {
        string GetUserId();
        string GetUserEmail();
        string GetUserFullName();
        string GetUserProfilePic();
    }
}
