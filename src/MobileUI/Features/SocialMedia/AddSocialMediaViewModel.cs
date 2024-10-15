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
    private bool _isError;

    [ObservableProperty]
    private string _errorText;
    
    [ObservableProperty]
    private string _currentUrl;
    
    public AddSocialMediaViewModel(IUserService userService,
        ISnackbarService snackbarService,
        int socialMediaPlatformId,
        string currentUrl = null)
    {
        _userService = userService;
        _snackbarService = snackbarService;
        _platformId = socialMediaPlatformId;
        _currentUrl = currentUrl;
        
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
                Placeholder = url ?? "https://linkedin.com/in/[your-name]";
                ValidationPattern = "^https?://(www.)?linkedin.com/in/([a-zA-Z0-9._-]+)$";
                break;
            case Constants.SocialMediaPlatformIds.GitHub:
                PlatformName = "GitHub";
                Url = url ?? "https://github.com/";
                Placeholder = url ?? "https://github.com/[your-username]";
                ValidationPattern = "^https?://(www.)?github.com/([a-zA-Z0-9._-]+)$";
                break;
            case Constants.SocialMediaPlatformIds.Twitter:
                PlatformName = "Twitter";
                Url = url ?? "https://x.com/";
                Placeholder = url ?? "https://x.com/[your-username]";
                ValidationPattern = "^https?://(www.)?(twitter|x).com/([a-zA-Z0-9._-]+)$";
                break;
            case Constants.SocialMediaPlatformIds.Company:
                PlatformName = "Company";
                Url = url ?? "https://";
                Placeholder = url ?? "https://[your-website]";
                ValidationPattern = @"^https?://\S+";
                break;
        }
    }

    [RelayCommand]
    private async Task Connect()
    {
        IsError = false;
        if (string.IsNullOrWhiteSpace(InputText))
        {
            IsError = true;
            ErrorText = "URL cannot be empty";
            return;
        }

        if (!IsUrlValid())
        {
            IsError = true;
            ErrorText = "The URL is not valid";
            return;
        }

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

    private bool IsUrlValid()
    {
        var reg = new Regex(ValidationPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
        return reg.IsMatch(InputText);
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