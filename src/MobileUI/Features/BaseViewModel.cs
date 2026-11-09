using CommunityToolkit.Mvvm.ComponentModel;

namespace SSW.Rewards.Mobile.ViewModels;

public partial class BaseViewModel : ObservableObject
{
    [ObservableProperty]
    public partial bool IsBusy { get; set; }

    [ObservableProperty]
    public partial string Title { get; set; }

    public INavigation Navigation { get; set; }

    public Page ViewPage { get; set; }

    public BaseViewModel()
    {
        Title = string.Empty;
    }
}
