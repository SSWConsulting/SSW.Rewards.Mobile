using Mopups.Services;

namespace SSW.Rewards.PopupPages;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class DeleteProfilePage
{
    private readonly IUserService _userService;
    private readonly IFirebaseAnalyticsService _firebaseAnalyticsService;
    
    private string _userEmail;

    public string UserEmail
    {
        get
        {
            return _userEmail;
        }
        set
        {
            _userEmail = value;
            OnPropertyChanged();
        }
    }
    
    private Color _profileBackgroundColour;
    private Color _sswRedColour;
    
    public DeleteProfilePage(IUserService userService, IFirebaseAnalyticsService firebaseAnalyticsService)
    {
        InitializeComponent();
        _userService = userService;
        _firebaseAnalyticsService = firebaseAnalyticsService;

        GetColors();
        
        _userService.MyEmailObservable().Subscribe(x => UserEmail = x);
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
        if (!IsEmailValid())
        {
            return;
        }
        
        await MopupService.Instance.PopAllAsync();
        var sure = await Shell.Current.DisplayAlert("Delete Profile", "Are you sure you want to delete your profile and all associated data?", "Yes", "No");

        if (sure)
        {
            DeleteIndicator.IsVisible = true;
            var requestSubmitted = await _userService.DeleteProfileAsync();
            DeleteIndicator.IsVisible = false;

            if (requestSubmitted)
            {
                await Shell.Current.DisplayAlert("Request Submitted", "Your request has been received and you will be contacted within 5 business days. You will now be logged out.", "OK");
                App.NavigateToLoginPage();
            }
            else
            {
                await Shell.Current.DisplayAlert("Error", "There was an error submitting your request. Please try again later.", "OK");
            }
        }
    }

    private void EmailEntry_OnTextChanged(object sender, TextChangedEventArgs e)
    {
        var isEmailValid = IsEmailValid();
        DeleteButton.IsEnabled = isEmailValid;
        DeleteButton.BackgroundColor = isEmailValid ? _sswRedColour : _profileBackgroundColour;
        DeleteButton.TextColor = isEmailValid ? Colors.White : Colors.Grey;
    }

    private bool IsEmailValid()
    {
        return string.Equals(EmailEntry.Text, UserEmail, StringComparison.CurrentCultureIgnoreCase);
    }
    
    private void GetColors()
    {
        Application.Current.Resources.TryGetValue("ProfileBackground", out var profileBackground);
        Application.Current.Resources.TryGetValue("SSWRed", out var sswRed);
        
        _profileBackgroundColour = (Color)profileBackground;
        _sswRedColour = (Color)sswRed;
    }
}