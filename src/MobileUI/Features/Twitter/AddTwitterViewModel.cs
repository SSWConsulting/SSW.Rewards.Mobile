using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mopups.Services;
using SSW.Rewards.Mobile.Controls;

namespace SSW.Rewards.Mobile.ViewModels;

public partial class AddTwitterViewModel(IUserService userService, ISnackbarService snackbarService) : BaseViewModel
{
    private static string TwitterUrl => "https://twitter.com/";

    [ObservableProperty]
    private string _inputText;

    [ObservableProperty]
    private int _cursorPosition;

    [ObservableProperty]
    private bool _isError;

    [ObservableProperty]
    private string _errorText;

    public static string TwitterPlaceholder => $"{TwitterUrl}[your-username]";

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

        await AddTwitterProfile();
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
            InputText = TwitterUrl;
        }

        CursorPosition = InputText.Length; // For some reason, works only on Android
    }

    private bool IsUrlValid()
    {
        var urlParts = InputText.Split('/');
        var isValid = urlParts.Length == 4 &&
                      string.Equals(urlParts[0], "https:", StringComparison.InvariantCultureIgnoreCase) &&
                      urlParts[1] == string.Empty &&
                      string.Equals(urlParts[2], "twitter.com", StringComparison.InvariantCultureIgnoreCase) &&
                      !string.IsNullOrWhiteSpace(urlParts[3]);
        return isValid;
    }

    private async Task AddTwitterProfile()
    {
        IsBusy = true;
        var result = await userService.SaveSocialMedia(Constants.SocialMediaPlatformIds.Twitter, InputText);
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
                snackbarOptions.Message = "Thanks for connecting Twitter with SSW Rewards";
                await snackbarService.ShowSnackbar(snackbarOptions);
                await ClosePage();
                break;
            case false:
                snackbarOptions.Message = "Twitter profile has been successfully updated";
                await snackbarService.ShowSnackbar(snackbarOptions);
                await ClosePage();
                break;
            default:
                snackbarOptions.Message = "Couldn't connect your Twitter profile, please try again later";
                snackbarOptions.Glyph = "\uf36f"; // cross icon
                await snackbarService.ShowSnackbar(snackbarOptions);
                break;
        }

        IsBusy = false;
    }
}