using CommunityToolkit.Mvvm.ComponentModel;

namespace SSW.Rewards.Mobile.ViewModels;

public partial class BaseViewModel : ObservableObject
{

    [ObservableProperty]
    bool isBusy = false;

    [ObservableProperty]
    string title = string.Empty;

    public INavigation Navigation { get; set; }
    
    public Page ViewPage { get; set; }
}
