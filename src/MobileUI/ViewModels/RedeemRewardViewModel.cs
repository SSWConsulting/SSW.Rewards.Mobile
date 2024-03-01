using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mopups.Services;
using SSW.Rewards.Shared.DTOs.Rewards;

namespace SSW.Rewards.Mobile.ViewModels;

public partial class RedeemRewardViewModel(IUserService userService, IRewardService rewardService) : BaseViewModel
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
    private AddressForm _address = new ();
    
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
        _reward = reward;
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
    private async Task ConfirmClicked()
    {
        Address.ValidateCommand.Execute(null);
        
        if (!Address.HasErrors)
        {
            IsBusy = true;
            await rewardService.ClaimReward(new ClaimRewardDto()
            {
                Id = _reward.Id,
                InPerson = false,
                AddressLine1 = Address.Line1,
                AddressLine2 = Address.Line2,
                AddressPostcode = Address.Postcode,
                AddressSuburb = Address.Suburb,
                AddressState = Address.State
            });
            
            IsBusy = false;
            // TODO: Implement confirmation page here
            await MopupService.Instance.PopAsync();
        }
    }
}

public partial class AddressForm : ObservableValidator
{
    [Required]
    [ObservableProperty]
    private string _line1;
    
    [ObservableProperty]
    private string _line2;

    [Required]
    [ObservableProperty]
    private string _suburb;

    [Required]
    [ObservableProperty]
    private string _state;

    [Required]
    [ObservableProperty]
    private string _postcode;
    
    [ObservableProperty]
    bool _isLine1Valid;
    
    [ObservableProperty]
    bool _isSuburbValid;
    
    [ObservableProperty]
    bool _isStateValid;
    
    [ObservableProperty]
    bool _isPostcodeValid;
    
    [RelayCommand]
    void Validate()
    {
        ValidateAllProperties();

        IsLine1Valid = GetErrors(nameof(Line1)).Any();
        IsSuburbValid = GetErrors(nameof(Suburb)).Any();
        IsStateValid = GetErrors(nameof(State)).Any();
        IsPostcodeValid = GetErrors(nameof(Postcode)).Any();
    }
}