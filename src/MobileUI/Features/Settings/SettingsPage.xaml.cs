﻿namespace SSW.Rewards.Mobile.Pages;

public partial class SettingsPage
{
    private readonly IFirebaseAnalyticsService _firebaseAnalyticsService;
    private readonly SettingsViewModel _viewModel;

    public SettingsPage(SettingsViewModel viewModel, IFirebaseAnalyticsService firebaseAnalyticsService)
    {
        _viewModel = viewModel;
        BindingContext = viewModel;
        _firebaseAnalyticsService = firebaseAnalyticsService;
        Title = "SSW Rewards | My Settings";
        InitializeComponent();
        SetUpSocialMediaSection();
    }
        
    protected override void OnAppearing()
    {
        base.OnAppearing();
        _firebaseAnalyticsService.Log("SettingsPage");
    }

    private void SetUpSocialMediaSection()
    {
        // Section has to be set up in code as cells can't be adjusted dynamically in the XAML
        SocialMediaSection.Add(new TextCell { Text = "LinkedIn", TextColor = Colors.White, Command = _viewModel.AddLinkedInCommand });
        SocialMediaSection.Add(new TextCell { Text = "GitHub", TextColor = Colors.White, Command = _viewModel.AddGitHubCommand });
        SocialMediaSection.Add(new TextCell { Text = "Twitter", TextColor = Colors.White, Command = _viewModel.AddTwitterCommand });

        if (!_viewModel.IsStaff)
        {
            SocialMediaSection.Add(new TextCell { Text = "Company", TextColor = Colors.White, Command = _viewModel.AddCompanyCommand });
        }
    }
}