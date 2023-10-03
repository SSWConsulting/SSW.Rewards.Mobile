namespace SSW.Rewards.Mobile.Services;

public class RewardService : ApiBaseService, IRewardService
{
    private RewardClient _rewardClient;

    public RewardService(IHttpClientFactory clientFactory, ApiOptions options) : base(clientFactory, options)
    {
        _rewardClient = new RewardClient(BaseUrl, AuthenticatedClient);
    }

    public async Task<List<Reward>> GetRewards()
    {
        var rewardList = new List<Reward>();

        try
        {
            var rewards = await _rewardClient.ListAsync();

            foreach (var reward in rewards.Rewards)
            {
                rewardList.Add(new Reward
                {
                    Cost = reward.Cost,
                    Id = reward.Id,
                    ImageUri = reward.ImageUri,
                    Name = reward.Name
                });
            }

            return rewardList;
        }
        catch (ApiException e)
        {
            if (e.StatusCode == 401)
            {
                await App.Current.MainPage.DisplayAlert("Authentication Failure", "Looks like your session has expired. Choose OK to go back to the login screen.", "OK");
                await Application.Current.MainPage.Navigation.PushModalAsync<LoginPage>();
            }
            else
            {
                await App.Current.MainPage.DisplayAlert("Oops...", "There seems to be a problem loading the leaderboard. Please try again soon.", "OK");
            }
        }

        return rewardList;
    }

    public async Task<ClaimRewardResult> RedeemReward(Reward reward)
    {
        throw new NotImplementedException();
    }

    public async Task<ClaimRewardResult> RewardRewardWithQRCode(string QRCode)
    {
        throw new NotImplementedException();
    }
}
