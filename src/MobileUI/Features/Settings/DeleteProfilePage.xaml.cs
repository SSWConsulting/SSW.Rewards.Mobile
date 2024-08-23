using Mopups.Pages;
using Mopups.Services;

namespace SSW.Rewards.PopupPages;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class DeleteProfilePage : PopupPage
{
    private readonly IUserService _userService;
    private readonly IFirebaseAnalyticsService _firebaseAnalyticsService;
    public DeleteProfilePage(IUserService userService, IFirebaseAnalyticsService firebaseAnalyticsService)
    {
        InitializeComponent();
        _userService = userService;
        _firebaseAnalyticsService = firebaseAnalyticsService;
    }
    
    protected override void OnAppearing()
    {
        base.OnAppearing();
        _firebaseAnalyticsService.Log("DeleteProfilePage");
    }

    private async void OnCancelTapped(object sender, System.EventArgs e)
    {
        await MopupService.Instance.PopAllAsync();
    }

    private async void OnDeleteTapped(object sender, System.EventArgs e)
    {
        var sure = await App.Current.MainPage.DisplayAlert("Delete Profile", "Are you sure you want to delete your profile and all associated data?", "Yes", "No");

        if (sure)
        {
            DeleteIndicator.IsVisible = true;
            var requestSubmitted = await _userService.DeleteProfileAsync();
            DeleteIndicator.IsVisible = false;

            if (requestSubmitted)
            {
                await App.Current.MainPage.DisplayAlert("Request Submitted", "Your request has been received and you will be contacted within 5 business days. You will now be logged out.", "OK");
                await Navigation.PushModalAsync<LoginPage>();
                await MopupService.Instance.PopAllAsync();
            }
            else
            {
                await App.Current.MainPage.DisplayAlert("Error", "There was an error submitting your request. Please try again later.", "OK");
            }
        }
    }
}