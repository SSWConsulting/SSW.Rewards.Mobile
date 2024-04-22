using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mopups.Services;

namespace SSW.Rewards.Mobile.ViewModels;

public partial class AddLinkedInViewModel : BaseViewModel
{
    [ObservableProperty]
    private bool _isOverlayVisible;

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

        await AddLinkedInProfile();
    }

    [RelayCommand]
    private async Task ClosePage()
    {
        IsOverlayVisible = false;
        await MopupService.Instance.PopAsync();
    }

    [RelayCommand]
    private void InputFocused()
    {
        if (string.IsNullOrWhiteSpace(InputText))
        {
            InputText = "https://linkedin.com/in/";
        }

        CursorPosition = InputText.Length; // For some reason, works only on Android
    }

    private bool IsUrlValid()
    {
        var urlParts = InputText.Split('/');
        var isValid = urlParts.Length == 5 &&
                      string.Equals(urlParts[0], "https:", StringComparison.InvariantCultureIgnoreCase) &&
                      urlParts[1] == string.Empty &&
                      string.Equals(urlParts[2], "linkedin.com", StringComparison.InvariantCultureIgnoreCase) &&
                      string.Equals(urlParts[3], "in", StringComparison.InvariantCultureIgnoreCase ) &&
                      !string.IsNullOrWhiteSpace(urlParts[4]);
        return isValid;
    }

    private async Task AddLinkedInProfile()
    {

    }
}