﻿using System.Diagnostics;
using CommunityToolkit.Mvvm.Messaging;
using SSW.Rewards.Mobile.Messages;

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
        Routing.RegisterRoute("earn/details", typeof(QuizDetailsPage));
        Routing.RegisterRoute("scan", typeof(ScanPage));
    }
    
    protected override bool OnBackButtonPressed()
    {
        if (Application.Current.MainPage.GetType() == typeof(AppShell) && Current.Navigation.NavigationStack.Where(x => x != null).Any())
        {
            return base.OnBackButtonPressed();
        }

        Process.GetCurrentProcess().CloseMainWindow();
        return true;
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

    private void OnNavigating(object sender, ShellNavigatingEventArgs e)
    {
        WeakReferenceMessenger.Default.Send(new TopBarAvatarMessage(AvatarOptions.Original));
        WeakReferenceMessenger.Default.Send(new TopBarTitleMessage(string.Empty));
    }
}
