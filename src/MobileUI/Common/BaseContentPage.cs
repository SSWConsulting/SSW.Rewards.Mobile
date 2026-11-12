using AsyncAwaitBestPractices;
using CommunityToolkit.Maui;
using CommunityToolkit.Mvvm.Messaging;

namespace SSW.Rewards.Mobile.Common;

public partial class BaseContentPage : ContentPage
{
    // Reconnects already handled by this page; -1 until it first appears.
    private int _handledReconnects = -1;

    [BindableProperty]
    public partial bool ShowBackButton { get; set; }

    protected override void OnAppearing()
    {
        if (AppShell.Current.Navigation.NavigationStack.Count > 1)
        {
            ShowBackButton = true;
        }

        base.OnAppearing();

        if (BindingContext is IRefreshOnReconnect && Monitor is { } monitor)
        {
            // Listen only while visible; a hidden page catches up below when it reappears.
            WeakReferenceMessenger.Default.Register<BaseContentPage, ConnectivityRestoredMessage>(
                this, static (page, _) => page.RefreshAfterReconnect());

            var missedReconnect = _handledReconnects >= 0 && monitor.ReconnectCount > _handledReconnects;
            _handledReconnects = monitor.ReconnectCount;
            if (missedReconnect)
            {
                RefreshAfterReconnect();
            }
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        WeakReferenceMessenger.Default.Unregister<ConnectivityRestoredMessage>(this);
    }

    private static ConnectivityMonitor Monitor =>
        IPlatformApplication.Current?.Services.GetService<ConnectivityMonitor>();

    private void RefreshAfterReconnect()
    {
        if (Monitor is { } monitor)
        {
            _handledReconnects = monitor.ReconnectCount;
        }

        if (BindingContext is IRefreshOnReconnect refreshable)
        {
            refreshable.RefreshOnReconnectAsync().SafeFireAndForget();
        }
    }
}
