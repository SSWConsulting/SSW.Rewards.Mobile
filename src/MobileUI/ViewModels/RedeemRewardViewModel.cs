using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mopups.Services;
using SSW.Rewards.ApiClient.Services;
using SSW.Rewards.Shared.DTOs.AddressTypes;
using SSW.Rewards.Shared.DTOs.Rewards;
using IRewardService = SSW.Rewards.Mobile.Services.IRewardService;
using IUserService = SSW.Rewards.Mobile.Services.IUserService;

namespace SSW.Rewards.Mobile.ViewModels;

public partial class RedeemRewardViewModel(IUserService userService, IRewardService rewardService, IAddressService addressService) : BaseViewModel
{
    private Reward _reward;
    
    [ObservableProperty]
    private string _image;
    
    [ObservableProperty]
    private string _name;
    
    [ObservableProperty]
    private string _description;
    
    [ObservableProperty]
    private int _cost;
    
    [ObservableProperty]
    private int _userBalance;

    [ObservableProperty]
    private bool _isBalanceVisible = true;
    
    [ObservableProperty]
    private bool _isAddressVisible;

    [ObservableProperty]
    private bool _isSearching;
    
    public ObservableCollection<Address> SearchResults { get; set; } = [];
    
    [ObservableProperty]
    private Address? _selectedAddress;
    
    [ObservableProperty]
    private bool _confirmEnabled = false;
    
    [ObservableProperty]
    private bool _isAddressEditExpanded = false;
    

    public void Initialise(Reward reward)
    {
        _reward = reward;
        Image = reward.ImageUri;
        Name = reward.Name;
        Description = reward.Description;
        Cost = reward.Cost;
        
        UserBalance = userService.MyBalance;
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
    private void ClosePopup()
    {
        MopupService.Instance.PopAsync();
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
    
    [RelayCommand(CanExecute = nameof(ConfirmClickedIsExecutable))]
    private async Task ConfirmClicked()
    {
        if (SelectedAddress is null)
        {
            return;
        }
        
        IsBusy = true;
        
        var claimResult = await rewardService.ClaimReward(new ClaimRewardDto()
        {
            Id = _reward.Id,
            InPerson = false,
            Address = SelectedAddress
        });
            
        IsBusy = false;
        
        // TODO: Implement confirmation page here
        await MopupService.Instance.PopAsync();
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
    
    private bool ConfirmClickedIsExecutable()
    {
        return ConfirmEnabled;
    }
    
}