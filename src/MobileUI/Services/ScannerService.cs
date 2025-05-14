using SSW.Rewards.ApiClient.Services;
using SSW.Rewards.Enums;
using SSW.Rewards.Mobile.Config;
using SSW.Rewards.Shared.DTOs.Rewards;

namespace SSW.Rewards.Mobile.Services;

public interface IScannerService
{
    bool IsValidQRCodeAsync(string qrCodeData);
    Task<ScanResponseViewModel> ValidateQRCodeAsync(string achievementString);
}

public class ScannerService : IScannerService
{
    private readonly IAchievementService _achievementClient;
    private readonly IRewardService _rewardService;
    private readonly IUserService _userService;
    private readonly ScannerConfig _scannerConfig;

    private bool _isStaff;

    public ScannerService(IRewardService rewardService, IAchievementService achievementClient, IUserService userService, ScannerConfig scannerConfig)
    {
        _rewardService = rewardService;
        _achievementClient = achievementClient;
        _userService = userService;
        _scannerConfig = scannerConfig;
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
                    case ClaimAchievementStatus.Claimed:
                        vm.result = ScanResult.Added;
                        vm.Title = response.viewModel.Name;
                        vm.Points = response.viewModel.Value;
                        vm.ScannedUserId = response.viewModel?.UserId;
                        break;

                    case ClaimAchievementStatus.Duplicate:
                        vm.result = ScanResult.Duplicate;
                        vm.Title = "Duplicate";
                        vm.Points = 0;
                        vm.ScannedUserId = response.viewModel?.UserId;
                        break;

                    case ClaimAchievementStatus.Error:
                        vm.result = ScanResult.Error;
                        vm.Title = "Error";
                        vm.Points = 0;
                        break;

                    case ClaimAchievementStatus.NotFound:
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
            if (!await ExceptionHandler.HandleApiException(e))
            {
                vm.result = ScanResult.Error;
            }
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
            if (!await ExceptionHandler.HandleApiException(e))
            {
                vm.result = ScanResult.Error;
            }
        }

        return vm;
    }

    public bool IsValidQRCodeAsync(string qrCodeData)
    {
        if (!_scannerConfig.ValidateBeforeProcessingQRCode)
        {
            // Disable QR code validation before processing.
            return true;
        }

        var (decodedQR, _) = DecodeQRCode(qrCodeData);
        return !string.IsNullOrWhiteSpace(decodedQR)
            && ApiClientConstants.SupportedQRCodeBodyPrefixes.Any(decodedQR.StartsWith);
    }

    public async Task<ScanResponseViewModel> ValidateQRCodeAsync(string rawQRCodeData)
    {
        var (decodedQR, qrCodeData) = DecodeQRCode(rawQRCodeData);
        if (decodedQR.StartsWith(ApiClientConstants.RewardsQRCodeAchievementPrefix))
        {
            return await PostAchievementAsync(qrCodeData);
        }

        if (decodedQR.StartsWith(ApiClientConstants.RewardsQRCodeRewardPrefix))
        {
            return await PostRewardAsync(qrCodeData);
        }
        
        if (decodedQR.StartsWith(ApiClientConstants.RewardsQRCodePendingRewardPrefix))
        {
            return _isStaff switch
            {
                true => await PostRewardAsync(qrCodeData, true),
                false => ScanResponseViewModel.OnlyStaffCanRedeemPendingRewards()
            };
        }

        return ScanResponseViewModel.NotFound();
    }

    private static (string decodedCode, string codeToSend) DecodeQRCode(string qrCodeData)
    {
        if (Uri.TryCreate(qrCodeData, UriKind.Absolute, out Uri uri))
        {
            if (uri.Scheme == ApiClientConstants.RewardsQRCodeProtocol ||
                uri.Host == ApiClientConstants.RewardsWebDomain)
            {
                var queryDictionary = System.Web.HttpUtility.ParseQueryString(uri.Query);
                qrCodeData = queryDictionary.Get(ApiClientConstants.RewardsQRCodeProtocolQueryName);
            }
        }

        var decodedQR = StringHelpers.Base64Decode(qrCodeData)?.ToLowerInvariant();
        return (decodedQR, qrCodeData);
    }
}
