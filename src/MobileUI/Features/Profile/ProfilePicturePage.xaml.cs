using CommunityToolkit.Maui.Behaviors;
using CommunityToolkit.Maui.Core;

namespace SSW.Rewards.PopupPages;

public partial class ProfilePicturePage
{
    private readonly Color _parentPageStatusBarColor;

    public ProfilePicturePage(ProfilePictureViewModel profilePictureViewModel, Color parentPageStatusBarColor = null)
    {
        _parentPageStatusBarColor = parentPageStatusBarColor ?? Colors.Black;
        InitializeComponent();
        profilePictureViewModel.Navigation = Navigation;
        BindingContext = profilePictureViewModel;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        // Change status bar back
        this.Behaviors.Clear();
        this.Behaviors.Add(new StatusBarBehavior
        {
            StatusBarColor = _parentPageStatusBarColor,
            StatusBarStyle = StatusBarStyle.LightContent,
        });
    }
}
