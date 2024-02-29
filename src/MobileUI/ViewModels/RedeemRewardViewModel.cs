using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mopups.Services;

namespace SSW.Rewards.Mobile.ViewModels;

public partial class RedeemRewardViewModel(IUserService userService) : BaseViewModel
{
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
    
    public string AddressLine1 { get; set; }
    public string AddressLine2 { get; set; }
    public string AddressSuburb { get; set; }
    public string AddressState { get; set; }
    public string AddressPostcode { get; set; }
    
    public ObservableCollection<string> States { get; set; } =
    [
        "NSW",
        "VIC",
        "QLD",
        "SA",
        "WA",
        "TAS",
        "NT",
        "ACT"
    ];

    public void Initialise(Reward reward)
    {
        Image = reward.ImageUri;
        Name = reward.Name;
        Description = reward.Description;
        Cost = reward.Cost;
        
        UserBalance = userService.MyBalance;
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
    private void ConfirmClicked()
    {
    }
}