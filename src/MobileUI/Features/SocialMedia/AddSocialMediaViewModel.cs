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
    private int _platformId;

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

    public AddSocialMediaViewModel(IUserService userService,
        ISnackbarService snackbarService,
        int socialMediaPlatformId)
    {
        _userService = userService;
        _snackbarService = snackbarService;
        _platformId = socialMediaPlatformId;
        
        Initialise(socialMediaPlatformId);
    }

    private void Initialise(int socialMediaPlatformId)
    {
        switch (socialMediaPlatformId)
        {
            case Constants.SocialMediaPlatformIds.LinkedIn:
                PlatformName = "LinkedIn";
                Url = "https://www.linkedin.com/in/";
                Placeholder = "https://www.linkedin.com/in/[your-name]";
                ValidationPattern = "^https://linkedin.com/in/([a-zA-Z0-9._-]+)$";
                break;
            case Constants.SocialMediaPlatformIds.GitHub:
                PlatformName = "GitHub";
                Url = "https://github.com/";
                Placeholder = "https://github.com/[your-username]";
                ValidationPattern = "^https://github.com/([a-zA-Z0-9._-]+)$";
                break;
            case Constants.SocialMediaPlatformIds.Twitter:
                PlatformName = "Twitter";
                Url = "https://x.com/";
                Placeholder = "https://x.com/[your-username]";
                ValidationPattern = "^https://(twitter|x).com/([a-zA-Z0-9._-]+)$";
                break;
            case Constants.SocialMediaPlatformIds.Company:
                PlatformName = "Company";
                Url = "https://";
                Placeholder = "https://[your-website]";
                ValidationPattern = @"^https://\S+";
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

        CursorPosition = InputText.Length; // For some reason, works only on Android
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