using SSW.Rewards.Shared.DTOs.Rewards;
using IApiRewardService = SSW.Rewards.ApiClient.Services.IRewardService;

namespace SSW.Rewards.Mobile.Services;

public class RewardService : IRewardService
{
    private readonly IApiRewardService _rewardClient;

    public RewardService(IApiRewardService rewardClient)
    {
        _rewardClient = rewardClient;
    }

    public async Task<List<Reward>> GetRewards()
    {
        var rewardList = new List<Reward>();

        try
        {
            var rewards = await _rewardClient.GetRewards(CancellationToken.None);

            foreach (var reward in rewards.Rewards)
            {
                rewardList.Add(new Reward
                {
                    Cost = reward.Cost,
                    Id = reward.Id,
                    ImageUri = reward.ImageUri,
                    Name = reward.Name,
                    Description = reward.Description,
                    CarouselImageUri = reward.CarouselImageUri,
                    IsCarousel = reward.IsCarousel
                });
            }

            return rewardList;
        }
        catch (Exception e)
        {
            if (!await ExceptionHandler.HandleApiException(e))
            {
                await App.Current.MainPage.DisplayAlert("Oops...", "There seems to be a problem loading the leaderboard. Please try again soon.", "OK");
            }
        }

        return rewardList;
    }
}
