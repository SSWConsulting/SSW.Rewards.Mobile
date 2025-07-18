﻿using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mopups.Services;
using Plugin.Maui.ScreenBrightness;
using SSW.Rewards.ApiClient.Services;
using SSW.Rewards.Enums;
using SSW.Rewards.Shared.DTOs.AddressTypes;
using SSW.Rewards.Shared.DTOs.Rewards;
using IRewardService = SSW.Rewards.Mobile.Services.IRewardService;
using IUserService = SSW.Rewards.Mobile.Services.IUserService;

namespace SSW.Rewards.Mobile.ViewModels;

public partial class RedeemRewardViewModel(
    IUserService userService,
    IRewardService rewardService,
    IAddressService addressService,
    IFirebaseAnalyticsService firebaseAnalyticsService) : BaseViewModel
{
    private Reward _reward;
    private float _defaultBrightness;
    private const float MaxBrightness = 1.0f;

    [ObservableProperty]
    private string _image;

    [ObservableProperty]
    private string _heading;

    [ObservableProperty]
    private string _description;

    [ObservableProperty]
    private int _cost;

    [ObservableProperty]
    private bool _isDigital;

    [ObservableProperty]
    private int _userBalance;
    
    [ObservableProperty]
    private bool _isHeaderVisible = true;

    [ObservableProperty]
    private bool _isBalanceVisible = true;

    [ObservableProperty]
    private bool _isQrCodeVisible;

    [ObservableProperty]
    private bool _isAddressVisible;

    [ObservableProperty]
    private bool _isSearching;

    [ObservableProperty]
    private Address? _selectedAddress;

    [ObservableProperty]
    private bool _confirmEnabled;

    [ObservableProperty]
    private bool _isAddressEditExpanded;

    [ObservableProperty]
    private bool _sendingClaim;

    [ObservableProperty]
    private bool _claimError;

    [ObservableProperty]
    private bool _claimSuccess;

    [ObservableProperty]
    private ImageSource _qrCode;

    public ObservableCollection<Address> SearchResults { get; set; } = [];
    public bool ShouldCallCallback { get; set; }

    public void Initialise(Reward reward)
    {
        _reward = reward;
        _defaultBrightness = ScreenBrightness.Default.Brightness;

        SetRewardProperties(reward);

        if (reward.IsPendingRedemption)
        {
            ShowQrCode();
        }

        userService.MyBalanceObservable().Subscribe(myBalance => UserBalance = myBalance);
        LogEvent(Constants.AnalyticsEvents.RewardView);
    }

    private void SetRewardProperties(Reward reward)
    {
        Image = reward.ImageUri;
        Heading = $"You are about to get:{Environment.NewLine}{reward.Name}";
        Description = reward.Description;
        Cost = reward.Cost;
        IsDigital = reward.IsDigital;
    }

    public void OnDisappearing()
    {
        ScreenBrightness.Default.Brightness = _defaultBrightness;
    }

    private void ShowQrCode(string qrCode = null)
    {
        ScreenBrightness.Default.Brightness = MaxBrightness;
        
        IsHeaderVisible = false;
        IsBalanceVisible = false;
        ConfirmEnabled = false;
        Heading = $"Ready to claim:{Environment.NewLine}{_reward.Name}";
        QrCode = ImageHelpers.GenerateQrCode(qrCode ?? _reward.PendingRedemptionCode);
        IsQrCodeVisible = true;
        ShouldCallCallback = true;
    }

    private void ShowClaimingState()
    {
        IsBalanceVisible = false;
        ConfirmEnabled = false;
        SendingClaim = true;
        Heading = "Claiming reward...";
    }

    private void ShowSuccessState(string message)
    {
        SendingClaim = false;
        Heading = "Success!";
        Description = message;
        ClaimSuccess = true;
    }

    private void ShowErrorState()
    {
        SendingClaim = false;
        Heading = "Error";
        Description = "Something went wrong - please try again later";
        ClaimError = true;
    }

    private void LogEvent(string eventName)
    {
        firebaseAnalyticsService.Log(eventName, new Dictionary<string, object>
        {
            { "reward_id", _reward.Id.ToString() },
            { "reward_name", _reward.Name },
            { "reward_value", _reward.Cost.ToString() }
        });
    }
    
    [RelayCommand]
    private async Task SearchAddress(string addressQuery)
    {
        IsSearching = true;

        SearchResults.Clear();

        if (string.IsNullOrEmpty(addressQuery))
        {
            IsSearching = false;
            return;
        }

        var results = await addressService.Search(addressQuery);

        results = results.ToList();

        if (results.Any())
        {
            foreach (var result in results)
            {
                SearchResults.Add(result);
            }
        }

        IsSearching = false;
    }

    [RelayCommand]
    private static async Task ClosePopup()
    {
        await MopupService.Instance.PopAsync();
    }

    [RelayCommand]
    private void NextClicked()
    {
        IsBalanceVisible = false;
        IsAddressVisible = true;
    }

    [RelayCommand]
    private void AddressSelected()
    {
        ConfirmEnabled = SelectedAddress != null;

        if (!ConfirmEnabled)
        {
            return;
        }

        IsAddressVisible = false;
    }

    [RelayCommand]
    private async Task RedeemInPersonClicked()
    {
        ShowClaimingState();

        var claimResult = await rewardService.CreatePendingRedemption(new CreatePendingRedemptionDto
        {
            Id = _reward.Id
        });

        if (claimResult.status == RewardStatus.Pending)
        {
            ShowQrCode(claimResult.Code);
            LogEvent(Constants.AnalyticsEvents.RewardRedemptionPending);
        }
        else
        {
            ShowErrorState();
        }
    }

    [RelayCommand]
    private async Task RedeemDigitalClicked()
    {
        var isConfirmed = await ViewPage.DisplayAlert(
            "Confirm Digital Redemption",
            $"Are you sure you want to redeem '{_reward.Name}' for {_reward.Cost:n0} points?\n\nThis digital reward will be sent to your email address.",
            "Yes, Redeem",
            "Cancel");

        if (!isConfirmed)
            return;

        ShowClaimingState();

        var claimResult = await rewardService.ClaimReward(new ClaimRewardDto()
        {
            Id = _reward.Id,
            InPerson = false
        });

        if (claimResult.status == RewardStatus.Claimed)
        {
            ShowSuccessState("Your digital reward will be sent to your email!");
            LogEvent(Constants.AnalyticsEvents.RewardRedeemed);
        }
        else
        {
            ShowErrorState();
        }
    }

    [RelayCommand]
    private async Task CancelPendingRedemptionClicked()
    {
        var isConfirmed = await ViewPage.DisplayAlert("Cancel",
            "Are you sure you want to cancel the pending redemption?", "Yes", "No");

        if (!isConfirmed)
            return;

        IsBusy = true;

        await rewardService.CancelPendingRedemption(new CancelPendingRedemptionDto
        {
            Id = _reward.Id
        });

        IsBusy = false;
        await MopupService.Instance.PopAsync();
        LogEvent(Constants.AnalyticsEvents.RewardRedemptionCancelled);
    }

    [RelayCommand]
    private async Task ConfirmClicked()
    {
        if (SelectedAddress is null)
        {
            return;
        }

        var isConfirmed = await ViewPage.DisplayAlert(
            "Confirm Physical Reward",
            $"Are you sure you want to redeem '{_reward.Name}' for {_reward.Cost:n0} points?\n\nThis reward will be shipped to:\n{SelectedAddress.freeformAddress}, Australia",
            "Yes, Ship It",
            "Cancel");

        if (!isConfirmed)
            return;

        ShowClaimingState();

        var claimResult = await rewardService.ClaimReward(new ClaimRewardDto()
        {
            Id = _reward.Id,
            InPerson = false,
            Address = SelectedAddress
        });

        if (claimResult.status == RewardStatus.Claimed)
        {
            ShowSuccessState("Your reward is on the way!");
            LogEvent(Constants.AnalyticsEvents.RewardRedeemed);
        }
        else
        {
            ShowErrorState();
        }
    }

    [RelayCommand]
    private void EditAddress()
    {
        IsAddressEditExpanded = !IsAddressEditExpanded;
    }

    [RelayCommand]
    private void SearchAgain()
    {
        ConfirmEnabled = false;
        IsAddressVisible = true;
    }
}