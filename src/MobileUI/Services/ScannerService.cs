using CommunityToolkit.Mvvm.Messaging;
using SSW.Rewards.ApiClient.Services;
using SSW.Rewards.Enums;
using SSW.Rewards.Mobile.Messages;
using SSW.Rewards.Shared.DTOs.Rewards;
using IApiRewardService = SSW.Rewards.ApiClient.Services.IRewardService;

namespace SSW.Rewards.Mobile.Services;

public class ScannerService : IScannerService
{
    private readonly IAchievementService _achievementClient;

    private readonly IApiRewardService _rewardClient;

    public ScannerService(IApiRewardService rewardClient, IAchievementService achievementClient)
    {
        _rewardClient = rewardClient;
        _achievementClient = achievementClient;
    }

    private async Task<ScanResponseViewModel> PostAchievementAsync(string achievementString)
    {
        ScanResponseViewModel vm = new()
        {
            ScanType = ScanType.Achievement
        };

        try
        {
            var response = await _achievementClient.ClaimAchievement(achievementString, CancellationToken.None);

            if (response != null)
            {
                switch (response.status)
                {
                    case Enums.ClaimAchievementStatus.Claimed:
                        vm.result = ScanResult.Added;
                        vm.Title = response.viewModel.Name;
                        vm.Points = response.viewModel.Value;
                        break;

                    case Enums.ClaimAchievementStatus.Duplicate:
                        vm.result = ScanResult.Duplicate;
                        vm.Title = "Duplicate";
                        vm.Points = 0;
                        break;

                    case Enums.ClaimAchievementStatus.Error:
                        vm.result = ScanResult.Error;
                        vm.Title = "Error";
                        vm.Points = 0;
                        break;

                    case Enums.ClaimAchievementStatus.NotFound:
                        vm.result = ScanResult.NotFound;
                        vm.Title = "Unrecognised";
                        vm.Points = 0;
                        break;
                }
            }
            else
                vm.result = ScanResult.Error;
        }
        catch (Exception e)
        {
            if (! await ExceptionHandler.HandleApiException(e))
            {
                vm.result = ScanResult.Error;
            }
        }
        catch
        {
            vm.result = ScanResult.Error;
        }

        return vm;
    }

    private async Task<ScanResponseViewModel> PostRewardAsync(string rewardString)
    {
        ScanResponseViewModel vm = new()
        {
            ScanType = ScanType.Reward
        };

        ClaimRewardDto claim = new() { Code = rewardString, InPerson = true };

        try
        {
            var response = await _rewardClient.RedeemReward(claim, CancellationToken.None);

            if (response != null)
            {
                switch (response.status)
                {
                    case RewardStatus.Claimed:
                        vm.result = ScanResult.Added;
                        vm.Title = response.Reward.Name;
                        break;

                    case RewardStatus.Duplicate:
                        vm.result = ScanResult.Duplicate;
                        vm.Title = "Duplicate";
                        break;

                    case RewardStatus.Error:
                        vm.result = ScanResult.Error;
                        vm.Title = "Error";
                        break;

                    case RewardStatus.NotFound:
                        vm.result = ScanResult.NotFound;
                        vm.Title = "Unrecognised";
                        break;

                    case RewardStatus.NotEnoughPoints:
                        vm.result = ScanResult.InsufficientBalance;
                        vm.Title = "Not enough points";
                        break;

                    default:
                        vm.result = ScanResult.Error;
                        vm.Title = "Error";
                        break;
                }
            }
            else
            {
                vm.result = ScanResult.Error;
            }
        }
        catch (Exception e)
        {
            if (! await ExceptionHandler.HandleApiException(e))
            {
                vm.result = ScanResult.Error;
            }
        }
        catch
        {
            vm.result = ScanResult.Error;
        }

        return vm;
    }

    public async Task<ScanResponseViewModel> ValidateQRCodeAsync(string qrCodeData)
    {
        if (qrCodeData.StartsWith("ach:"))
        {
            return await PostAchievementAsync(qrCodeData);
        }

        if (qrCodeData.StartsWith("rwd:"))
        {
            return await PostRewardAsync(qrCodeData);
        }

        return new ScanResponseViewModel
        {
            result = ScanResult.NotFound,
            Title = "Unrecognised"
        };
    }
}
