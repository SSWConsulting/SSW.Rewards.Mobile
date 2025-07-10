using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mopups.Services;
using SSW.Rewards.Mobile.Controls;
using SSW.Rewards.Shared.Utils;

namespace SSW.Rewards.Mobile.ViewModels;

public partial class AddSocialMediaViewModel : BaseViewModel
{
    private readonly IUserService _userService;
    private readonly ISnackbarService _snackbarService;
    private readonly int _platformId;

    private Regex _validationPattern;

    [ObservableProperty]
    private string _placeholder;
    
    [ObservableProperty]
    private string _url;

    [ObservableProperty]
    private string _platformName;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CompleteUrl))]
    private string _inputText;

    [ObservableProperty]
    private int _cursorPosition;

    [ObservableProperty]
    private string _errorText;
    
    [ObservableProperty]
    private string _currentUrl;
    
    [ObservableProperty]
    private string _icon;

    public string CompleteUrl
    {
        get
        {
            return string.IsNullOrEmpty(InputText) ? $"{Url}{Placeholder}" : $"{Url}{InputText}";
        }
    }

    public AddSocialMediaViewModel(IUserService userService,
        ISnackbarService snackbarService,
        int socialMediaPlatformId,
        string currentUrl)
    {
        _userService = userService;
        _snackbarService = snackbarService;
        _platformId = socialMediaPlatformId;

        Initialise(currentUrl, socialMediaPlatformId);
    }

    private void Initialise(string currentUrl, int socialMediaPlatformId)
    {
        switch (socialMediaPlatformId)
        {
            case Constants.SocialMediaPlatformIds.LinkedIn:
                PlatformName = "LinkedIn";
                Url = "https://linkedin.com/in/";
                Placeholder = "[your-name]";
                _validationPattern = RegexHelpers.LinkedInRegex();
                Icon = "\uf0e1";
                break;
            case Constants.SocialMediaPlatformIds.GitHub:
                PlatformName = "GitHub";
                Url = "https://github.com/";
                Placeholder = "[your-username]";
                _validationPattern = RegexHelpers.GitHubRegex();
                Icon = "\uf09b";
                break;
            case Constants.SocialMediaPlatformIds.Twitter:
                PlatformName = "Twitter";
                Url = "https://x.com/";
                Placeholder = "[your-username]";
                _validationPattern = RegexHelpers.TwitterRegex();
                Icon = "\ue61b";
                break;
            case Constants.SocialMediaPlatformIds.Company:
                PlatformName = "Company";
                Url = "https://";
                Placeholder = "[your-website]";
                _validationPattern = RegexHelpers.CompanyRegex();
                Icon = "\uf1ad";
                break;
        }

        CurrentUrl = currentUrl;

        if (!string.IsNullOrWhiteSpace(currentUrl))
        {
            InputText = _validationPattern.ExtractUsername(currentUrl);
        }
    }

    [RelayCommand]
    private async Task Connect()
    {
        InputText = InputText.Trim();

        var isValid = ValidateForm();

        if (!isValid) return;

        await AddProfile();
    }

    [RelayCommand]
    private static async Task ClosePage()
    {
        await MopupService.Instance.PopAsync();
    }

    [RelayCommand]
    private void InputFocused()
    {
        if (string.IsNullOrWhiteSpace(InputText)) 
        {
            InputText = Url;
        }

        App.Current.Dispatcher.Dispatch(() =>
        {
            CursorPosition = InputText.Length;
        });
    }

    [RelayCommand]
    private void InputUnfocused()
    {
        InputText = InputText.Trim();
    }
    
    [RelayCommand]
    private async Task OpenLink()
    {
        var isValid = ValidateForm();
        
        if (isValid && Uri.TryCreate(CompleteUrl, UriKind.Absolute, out Uri uri))
        {
            try
            {
                await Browser.Default.OpenAsync(uri, BrowserLaunchMode.External);
            }
            catch (Exception)
            {
                await Shell.Current.DisplayAlert("Error", "There was an error trying to launch the default browser.", "OK");
            }
        }
    }

    private bool IsUrlValid()
    {
        return _validationPattern.IsMatch(CompleteUrl);
    }

    private bool ValidateForm()
    {
        ErrorText = string.Empty;
        
        if (string.IsNullOrWhiteSpace(InputText))
        {
            ErrorText = "URL cannot be empty";
            return false;
        }

        if (!IsUrlValid())
        {
            ErrorText = "The URL is not valid";
            return false;
        }
        
        return true;
    }

    private async Task AddProfile()
    {
        IsBusy = true;
        var result = await _userService.SaveSocialMedia(_platformId, CompleteUrl);
        var snackbarOptions = new SnackbarOptions
        {
            Glyph = "\uf297", // tick icon
            ShowPoints = false,
        };
        switch (result)
        {
            case true:
                snackbarOptions.ShowPoints = true;
                snackbarOptions.Points = 150;
                snackbarOptions.Message = $"Thanks for connecting your {PlatformName} with SSW Rewards";
                await ClosePage();
                break;
            case false:
                snackbarOptions.Message = $"{PlatformName} has been successfully updated";
                await ClosePage();
                break;
            default:
                snackbarOptions.Message = $"Couldn't connect your {PlatformName}, please try again later";
                snackbarOptions.Glyph = "\uf36f"; // cross icon
                break;
        }

        IsBusy = false;
        await _snackbarService.ShowSnackbar(snackbarOptions);
    }
}