using CommunityToolkit.Maui.Behaviors;
using CommunityToolkit.Maui.Core;

namespace SSW.Rewards.Mobile.PopupPages;

public partial class AddLinkedInPage
{
    private readonly Color _parentPageStatusBarColor;

    public AddLinkedInPage(IUserService userService, ISnackbarService snackbarService, Color parentPageStatusBarColor = null)
    {
        _parentPageStatusBarColor = parentPageStatusBarColor ?? Colors.Black;
        InitializeComponent();
        BindingContext = new AddLinkedInViewModel(userService, snackbarService);
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