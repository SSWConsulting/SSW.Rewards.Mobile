using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mopups.Services;
using SSW.Rewards.Mobile.Controls;

namespace SSW.Rewards.Mobile.ViewModels;

public partial class AddSocialMediaViewModel(
    IUserService userService,
    ISnackbarService snackbarService,
    SocialMediaPlatform socialMediaPlatform)
    : BaseViewModel
{
    public string Placeholder { get; set; } = socialMediaPlatform.Placeholder;
    public string PlatformName { get; set; } = socialMediaPlatform.PlatformName;

    [ObservableProperty]
    private string _inputText;

    [ObservableProperty]
    private int _cursorPosition;

    [ObservableProperty]
    private bool _isError;

    [ObservableProperty]
    private string _errorText;

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
            InputText = socialMediaPlatform.Url;
        }

        CursorPosition = InputText.Length; // For some reason, works only on Android
    }

    private bool IsUrlValid()
    {
        var reg = new Regex(socialMediaPlatform.ValidationPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
        return reg.IsMatch(socialMediaPlatform.Url);
    }

    private async Task AddProfile()
    {
        IsBusy = true;
        var result = await userService.SaveSocialMedia(socialMediaPlatform.PlatformId, InputText);
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
                snackbarOptions.Message = $"Thanks for connecting your {socialMediaPlatform.PlatformName} with SSW Rewards";
                await snackbarService.ShowSnackbar(snackbarOptions);
                await ClosePage();
                break;
            case false:
                snackbarOptions.Message = $"{socialMediaPlatform.PlatformName} has been successfully updated";
                await snackbarService.ShowSnackbar(snackbarOptions);
                await ClosePage();
                break;
            default:
                snackbarOptions.Message = $"Couldn't connect your {socialMediaPlatform.PlatformName}, please try again later";
                snackbarOptions.Glyph = "\uf36f"; // cross icon
                await snackbarService.ShowSnackbar(snackbarOptions);
                break;
        }

        IsBusy = false;
    }
}