using System.Diagnostics;

#if IOS
using UIKit;
#endif

namespace SSW.Rewards.Mobile;

public partial class AppShell
{
    public AppShell()
    {
        BindingContext = this;
        InitializeComponent();

        // Register routes for navigation
        Routing.RegisterRoute("postdetail", typeof(PostDetailPage));
    }

    protected override bool OnBackButtonPressed()
    {
        // Close the flyout if it's open instead of closing the app
        if (Current.FlyoutIsPresented)
        {
            Current.FlyoutIsPresented = false;
            return true;
        }

        // Don't attempt to navigate back if there are open popups as this makes it go back twice.
        // This is due to this being called on both the main page and the popup page.
        if (Mopups.Services.MopupService.Instance.PopupStack.Any())
        {
            return false;
        }

        return base.OnBackButtonPressed();
    }

    private void Button_Clicked(object sender, EventArgs e)
    {
        Current.GoToAsync("//scan");
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

#if IOS
        // Workaround for filling the gap above our custom top bar on iOS
        // See: https://github.com/drasticactions/MauiRepros/blob/main/StatusBarHack/MainPage.xaml.cs#L30-L52
        if (this.GetParentWindow()?.Handler?.PlatformView is not UIWindow window)
        {
            return;
        }

        var topPadding = window?.SafeAreaInsets.Top ?? 0;
        Application.Current.Resources.TryGetValue("Background", out var background);

        if (background is not Color backgroundColor)
        {
            return;
        }

        var statusBar = new UIView(new CoreGraphics.CGRect(0, 0, UIKit.UIScreen.MainScreen.Bounds.Size.Width, topPadding))
        {
            BackgroundColor = UIColor.FromRGB(backgroundColor.Red, backgroundColor.Green, backgroundColor.Blue)
        };

        if (this.Handler?.PlatformView is UIView view)
        {
            view?.AddSubview(statusBar);
        }
#endif
    }
}
