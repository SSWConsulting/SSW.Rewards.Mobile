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
    private Address _selectedAddress;
    

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
    private async Task SearrhAddress(string addressQuery)
    {
        IsSearching = true;
        
        SearchResults.Clear();
        
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
    
    [RelayCommand(CanExecute = nameof(ConfirmClickedIsExecutable))]
    private async Task ConfirmClicked()
    {
        IsBusy = true;
        await rewardService.ClaimReward(new ClaimRewardDto()
        {
            Id = _reward.Id,
            InPerson = false,
            Address = _selectedAddress
        });
            
        IsBusy = false;
        // TODO: Implement confirmation page here
        await MopupService.Instance.PopAsync();
    }
    private bool ConfirmClickedIsExecutable()
    {
        return _selectedAddress != null;
    }
    
}