using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mopups.Services;
using SSW.Rewards.Mobile.Controls;

namespace SSW.Rewards.Mobile.ViewModels;

public partial class AddGitHubViewModel(IUserService userService, ISnackbarService snackbarService) : BaseViewModel
{
    private string GitHubUrl => "https://github.com/";

    [ObservableProperty]
    private string _inputText;

    [ObservableProperty]
    private int _cursorPosition;

    [ObservableProperty]
    private bool _isError;

    [ObservableProperty]
    private string _errorText;

    public string GitHubPlaceholder => $"{GitHubUrl}[your-username]";

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

        await AddGitHubProfile();
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
            InputText = GitHubUrl;
        }

        CursorPosition = InputText.Length; // For some reason, works only on Android
    }

    private bool IsUrlValid()
    {
        var urlParts = InputText.Split('/');
        var isValid = urlParts.Length == 4 &&
                      string.Equals(urlParts[0], "https:", StringComparison.InvariantCultureIgnoreCase) &&
                      urlParts[1] == string.Empty &&
                      string.Equals(urlParts[2], "github.com", StringComparison.InvariantCultureIgnoreCase) &&
                      !string.IsNullOrWhiteSpace(urlParts[3]);
        return isValid;
    }

    private async Task AddGitHubProfile()
    {
        IsBusy = true;
        var result = await userService.SaveSocialMedia(Constants.SocialMediaPlatformIds.GitHub, InputText);
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
                snackbarOptions.Message = "Thanks for connecting GitHub with SSW Rewards";
                await snackbarService.ShowSnackbar(snackbarOptions);
                await ClosePage();
                break;
            case false:
                snackbarOptions.Message = "GitHub profile has been successfully updated";
                await snackbarService.ShowSnackbar(snackbarOptions);
                await ClosePage();
                break;
            default:
                snackbarOptions.Message = "Couldn't connect your GitHub profile, please try again later";
                snackbarOptions.Glyph = "\uf36f"; // cross icon
                await snackbarService.ShowSnackbar(snackbarOptions);
                break;
        }

        IsBusy = false;
    }
}