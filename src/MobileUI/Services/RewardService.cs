
using SSW.Rewards.ApiClient.Services;
using SSW.Rewards.Enums;
using SSW.Rewards.Shared.DTOs.Rewards;
using IApiRewardService = SSW.Rewards.ApiClient.Services.IRewardService;

namespace SSW.Rewards.Mobile.Services;

public interface IRewardService
{
    Task<List<Reward>> GetRewards();
    Task <ClaimRewardResult> ClaimReward(ClaimRewardDto claim);
    Task<CreatePendingRedemptionResult> CreatePendingRedemption(CreatePendingRedemptionDto claim);
    Task<CancelPendingRedemptionResult> CancelPendingRedemption(CancelPendingRedemptionDto claim);
    Task<ClaimRewardResult> ClaimRewardForUser(ClaimRewardDto claim);
}

public class RewardService : IRewardService
{
    private readonly IApiRewardService _rewardClient;
    private readonly IRewardAdminService _adminRewardClient;
    private readonly IUserService _userService;

    public RewardService(IApiRewardService rewardClient, IUserService userService, IRewardAdminService adminRewardService)
    {
        _rewardClient = rewardClient;
        _userService = userService;
        _adminRewardClient = adminRewardService;
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
                    IsCarousel = reward.IsCarousel,
                    IsHidden = reward.IsHidden
                });
            }

            return rewardList;
        }
        catch (Exception e)
        {
            if (!await ExceptionHandler.HandleApiException(e))
            {
                await Shell.Current.DisplayAlert("Oops...", "There seems to be a problem loading the leaderboard. Please try again soon.", "OK");
            }
        }

        return rewardList;
    }

    public async Task<ClaimRewardResult> ClaimReward(ClaimRewardDto claim)
    {
        var result = new ClaimRewardResult() { status = RewardStatus.Error };

        try
        {
            result = await _rewardClient.RedeemReward(claim, CancellationToken.None);
        }
        catch (Exception e)
        {
            // TODO: Handle errors
            if (! await ExceptionHandler.HandleApiException(e))
            {
            }
        }

        await _userService.UpdateMyDetailsAsync();

        return result;
    }
    
    public async Task<ClaimRewardResult> ClaimRewardForUser(ClaimRewardDto claim)
    {
        var result = new ClaimRewardResult() { status = RewardStatus.Error };

        try
        {
            result = await _adminRewardClient.ClaimForUser(claim.Code, claim.UserId, claim.IsPendingRedemption, CancellationToken.None);
        }
        catch (Exception e)
        {
            await ExceptionHandler.HandleApiException(e);
        }

        return result;
    }
    
    public async Task<CreatePendingRedemptionResult> CreatePendingRedemption(CreatePendingRedemptionDto claim)
    {
        var result = new CreatePendingRedemptionResult { status = RewardStatus.Error };

        try
        {
            result = await _rewardClient.CreatePendingRedemption(claim, CancellationToken.None);
        }
        catch (Exception e)
        {
            await ExceptionHandler.HandleApiException(e);
        }

        await _userService.UpdateMyDetailsAsync();

        return result;
    }
    
    public async Task<CancelPendingRedemptionResult> CancelPendingRedemption(CancelPendingRedemptionDto claim)
    {
        var result = new CancelPendingRedemptionResult { Status = RewardStatus.Error };

        try
        {
            result = await _rewardClient.CancelPendingRedemption(claim, CancellationToken.None);
        }
        catch (Exception e)
        {
            await ExceptionHandler.HandleApiException(e);
        }

        return result;
    }
}
