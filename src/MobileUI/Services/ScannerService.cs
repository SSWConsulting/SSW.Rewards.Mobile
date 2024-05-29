using CommunityToolkit.Mvvm.Messaging;
using SSW.Rewards.ApiClient.Services;
using SSW.Rewards.Enums;
using SSW.Rewards.Mobile.Messages;
using SSW.Rewards.Shared.DTOs.Rewards;
using IApiRewardService = SSW.Rewards.ApiClient.Services.IRewardService;

namespace SSW.Rewards.Mobile.Services;

public interface IScannerService
{
    Task<ScanResponseViewModel> ValidateQRCodeAsync(string achievementString);
}

public class ScannerService : IScannerService
{
    private readonly IAchievementService _achievementClient;

    private readonly IRewardService _rewardService;

    private readonly IUserService _userService;

    private bool _isStaff;

    public ScannerService(IRewardService rewardService, IAchievementService achievementClient, IUserService userService)
    {
        _rewardService = rewardService;
        _achievementClient = achievementClient;
        _userService = userService;

        _userService.IsStaffObservable().Subscribe(isStaff => _isStaff = isStaff);
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

    private async Task<ScanResponseViewModel> PostRewardAsync(string rewardString, bool isPendingRedemption = false)
    {
        ScanResponseViewModel vm = new()
        {
            ScanType = ScanType.Reward
        };

        ClaimRewardDto claim = new() { Code = rewardString, InPerson = true, IsPendingRedemption = isPendingRedemption};

        try
        {
            var response = isPendingRedemption ? await _rewardService.ClaimRewardForUser(claim) : await _rewardService.ClaimReward(claim);

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
                    
                    case RewardStatus.Confirmed:
                        vm.result = ScanResult.Confirmed;
                        vm.Title = response.Reward.Name;
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
        var decodedQR = StringHelpers.Base64Decode(qrCodeData);
        
        if (decodedQR.StartsWith("ach:"))
        {
            return await PostAchievementAsync(qrCodeData);
        }

        if (decodedQR.StartsWith("rwd:"))
        {
            return await PostRewardAsync(qrCodeData);
        }
        
        if (decodedQR.StartsWith("pnd:"))
        {
            if (!_isStaff)
            {
                return new ScanResponseViewModel
                {
                    result = ScanResult.Error,
                    Title = "Only SSW staff can redeem pending rewards"
                };
            }
            
            return await PostRewardAsync(qrCodeData, true);
        }

        return new ScanResponseViewModel
        {
            result = ScanResult.NotFound,
            Title = "Unrecognised"
        };
    }
}
