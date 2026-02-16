using CommunityToolkit.Maui;

namespace SSW.Rewards.Mobile.Common;

public partial class BaseContentPage : ContentPage
{
    [BindableProperty]
    public partial bool ShowBackButton { get; set; }

    protected override void OnAppearing()
    {
        if (AppShell.Current.Navigation.NavigationStack.Count > 1)
        {
            ShowBackButton = true;
        }
        
        base.OnAppearing();
    }
}