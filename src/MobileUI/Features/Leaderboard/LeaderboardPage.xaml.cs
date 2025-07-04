﻿using Microsoft.Maui.Controls.Internals;

namespace SSW.Rewards.Mobile.Pages;

public partial class LeaderboardPage
{
    private readonly LeaderboardViewModel _viewModel;
    private readonly IFirebaseAnalyticsService _firebaseAnalyticsService;

    private bool _animationComplete = false;

    public LeaderboardPage(LeaderboardViewModel leaderboardViewModel, IFirebaseAnalyticsService firebaseAnalyticsService)
    {
        _viewModel = leaderboardViewModel;
        _viewModel.Navigation = Navigation;
        _firebaseAnalyticsService = firebaseAnalyticsService;
        BindingContext = _viewModel;
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _firebaseAnalyticsService.Log("LeaderboardPage");

        // Page might be ready a bit earlier due to cached leaderboard
        // while data is refreshed in the background.
        _viewModel.ReadyEarly += Animate;
        await _viewModel.Initialise();

        await Animate();

        _viewModel.ScrollTo += ScrollTo;
    }

    private void ScrollTo(int i)
    {
        LeadersCollection.ScrollTo(i, position: ScrollToPosition.Center, animate: false);
    }

    private async Task Animate()
    {
        if (_animationComplete)
        {
            return;
        }

        if (!_viewModel.IsRunning)
        {
            LeadersCollection.IsVisible = true;
            return;
        }

        _animationComplete = true;

        await Task.Delay(1000);
        LeadersCollection.Opacity = 0;
        LeadersCollection.IsVisible = true;
        await LeadersCollection.FadeTo(1, 800, Easing.CubicIn);
    }
    
    private void OnSizeChanged(object sender, EventArgs e)
    {
        // Fix for CollectionView.Header not resizing when window size changes
        if (DeviceInfo.Current.Platform == DevicePlatform.iOS && LeadersCollection.Header is VisualElement headerVisualElement)
        {
            headerVisualElement.InvalidateMeasure();
        }
    }
}