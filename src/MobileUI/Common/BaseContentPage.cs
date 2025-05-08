using Maui.BindableProperty.Generator.Core;

namespace SSW.Rewards.Mobile.Common;

public partial class BaseContentPage : ContentPage
{
    [AutoBindable]
    private bool _showBackButton = false;

    protected override void OnAppearing()
    {
        if (AppShell.Current.Navigation.NavigationStack.Count > 1)
        {
            ShowBackButton = true;
        }
        
        base.OnAppearing();
    }
}