using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mopups.Services;
using SSW.Rewards.Mobile.Controls;

namespace SSW.Rewards.Mobile.ViewModels;

public partial class AddSocialMediaViewModel : BaseViewModel
{
    private readonly IUserService _userService;
    private readonly ISnackbarService _snackbarService;
    private readonly int _platformId;

    [ObservableProperty]
    private string _placeholder;
    
    [ObservableProperty]
    private string _url;

    [ObservableProperty]
    private string _platformName;
    
    [ObservableProperty]
    private string _validationPattern;

    [ObservableProperty]
    private string _inputText;

    [ObservableProperty]
    private int _cursorPosition;

    [ObservableProperty]
    private string _errorText;
    
    [ObservableProperty]
    private string _successText;
    
    [ObservableProperty]
    private string _currentUrl;
    
    [ObservableProperty]
    private string _icon;
    
    public AddSocialMediaViewModel(IUserService userService,
        ISnackbarService snackbarService,
        int socialMediaPlatformId,
        string currentUrl)
    {
        _userService = userService;
        _snackbarService = snackbarService;
        _platformId = socialMediaPlatformId;
        
        CurrentUrl = currentUrl;
        
        if (!string.IsNullOrWhiteSpace(currentUrl))
        {
            InputText = currentUrl;
        }
        
        Initialise(socialMediaPlatformId);
    }

    private void Initialise(int socialMediaPlatformId)
    {
        var url = !string.IsNullOrEmpty(CurrentUrl) ? CurrentUrl : null;
        
        switch (socialMediaPlatformId)
        {
            case Constants.SocialMediaPlatformIds.LinkedIn:
                PlatformName = "LinkedIn";
                Url = url ?? "https://linkedin.com/in/";
                Placeholder = "https://linkedin.com/in/[your-name]";
                ValidationPattern = "^https?://(www.)?linkedin.com/in/([a-zA-Z0-9._-]+)$";
                Icon = "\uf0e1";
                break;
            case Constants.SocialMediaPlatformIds.GitHub:
                PlatformName = "GitHub";
                Url = url ?? "https://github.com/";
                Placeholder = "https://github.com/[your-username]";
                ValidationPattern = "^https?://(www.)?github.com/([a-zA-Z0-9._-]+)$";
                Icon = "\uf09b";
                break;
            case Constants.SocialMediaPlatformIds.Twitter:
                PlatformName = "Twitter";
                Url = url ?? "https://x.com/";
                Placeholder = "https://x.com/[your-username]";
                ValidationPattern = "^https?://(www.)?(twitter|x).com/([a-zA-Z0-9._-]+)$";
                Icon = "\ue61b";
                break;
            case Constants.SocialMediaPlatformIds.Company:
                PlatformName = "Company";
                Url = url ?? "https://";
                Placeholder = "https://[your-website]";
                ValidationPattern = @"^https?://\S+";
                Icon = "\uf1ad";
                break;
        }
    }

    [RelayCommand]
    private async Task Connect()
    {
        var isValid = ValidateForm();

        if (!isValid) return;
        
        SuccessText = "✅ Done";

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
    private async Task OpenLink()
    {
        var isValid = ValidateForm();
        
        if (isValid && Uri.TryCreate(InputText, UriKind.Absolute, out Uri uri))
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
        var reg = new Regex(ValidationPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
        return reg.IsMatch(InputText);
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
        var result = await _userService.SaveSocialMedia(_platformId, InputText);
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
                await _snackbarService.ShowSnackbar(snackbarOptions);
                await ClosePage();
                break;
            case false:
                snackbarOptions.Message = $"{PlatformName} has been successfully updated";
                await _snackbarService.ShowSnackbar(snackbarOptions);
                await ClosePage();
                break;
            default:
                snackbarOptions.Message = $"Couldn't connect your {PlatformName}, please try again later";
                snackbarOptions.Glyph = "\uf36f"; // cross icon
                await _snackbarService.ShowSnackbar(snackbarOptions);
                break;
        }

        IsBusy = false;
    }
}