using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Mopups.Services;
using SSW.Rewards.Mobile.Config;
using SSW.Rewards.Mobile.Controls;
using SSW.Rewards.Shared.Utils;

namespace SSW.Rewards.Mobile.ViewModels;

public partial class AddSocialMediaViewModel : BaseViewModel
{
    private readonly IUserService _userService;
    private readonly ISnackbarService _snackbarService;
    private readonly ILogger<AddSocialMediaViewModel> _logger;
    private readonly SocialMediaConfig _configuration;
    private readonly IAlertService _alertService;
    private int _platformId;
    private Regex _validationPattern;

    private const string TickIcon = "\uf297";
    private const string CrossIcon = "\uf36f";
    private const int NewConnectionPoints = 150;

    [ObservableProperty]
    private string _placeholder = string.Empty;

    [ObservableProperty]
    private string _url = string.Empty;

    [ObservableProperty]
    private string _platformName = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CompleteUrl))]
    [NotifyPropertyChangedFor(nameof(IsUrlValid))]
    private string _inputText = string.Empty;

    [ObservableProperty]
    private int _cursorPosition;

    [ObservableProperty]
    private string _errorText = string.Empty;

    [ObservableProperty]
    private string _currentUrl = string.Empty;

    [ObservableProperty]
    private string _icon = string.Empty;

    public string CompleteUrl =>
        string.IsNullOrEmpty(InputText) ? $"{Url}{Placeholder}" : $"{Url}{InputText}";

    public bool IsUrlValid
    {
        get
        {
            if (_validationPattern == null || string.IsNullOrWhiteSpace(CompleteUrl))
                return false;

            return _validationPattern.IsMatch(CompleteUrl);
        }
    }

    public AddSocialMediaViewModel(
        IUserService userService,
        ISnackbarService snackbarService,
        ILogger<AddSocialMediaViewModel> logger,
        SocialMediaConfig configuration,
        IAlertService alertService)
    {
        _userService = userService;
        _snackbarService = snackbarService;
        _logger = logger;
        _configuration = configuration;
        _alertService = alertService;
    }

    public async Task InitialiseAsync(string currentUrl, int socialMediaPlatformId)
    {
        try
        {
            if (!_configuration.Platforms.TryGetValue(socialMediaPlatformId, out var config))
            {
                _logger.LogError("Social media platform {PlatformId} not found", socialMediaPlatformId);
                await ShowErrorAndClose("Social media platform not found.");
                return;
            }

            _platformId = socialMediaPlatformId;
            _validationPattern = config.ValidationPattern();

            PlatformName = config.PlatformName;
            Url = config.Url;
            Placeholder = config.Placeholder;
            Icon = config.Icon;
            CurrentUrl = currentUrl;

            if (!string.IsNullOrWhiteSpace(currentUrl))
            {
                InputText = currentUrl;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing AddSocialMediaViewModel for platform {PlatformId}", socialMediaPlatformId);
            await ShowErrorAndClose("An error occurred while loading the social media platform.");
        }
    }

    partial void OnInputTextChanged(string value)
    {
        // Clear error when input is cleared
        if (string.IsNullOrWhiteSpace(value))
        {
            ErrorText = string.Empty;
            return;
        }

        // Check if the user pasted or entered a full URL and extract just the handle
        if (_validationPattern != null)
        {
            var extractedHandle = _validationPattern.ExtractHandle(value);
            if (!string.IsNullOrEmpty(extractedHandle) && extractedHandle != value)
            {
                InputText = extractedHandle;
                return; // Prevent clearing error text below since we're updating InputText
            }
        }

        // Clear error text when user starts typing
        if (!string.IsNullOrEmpty(ErrorText))
        {
            ErrorText = string.Empty;
        }
    }

    [RelayCommand]
    private async Task Connect()
    {
        if (IsBusy) return;

        InputText = InputText.Trim();

        if (!ValidateForm()) return;

        await SaveProfile();
    }

    [RelayCommand]
    private static async Task ClosePage()
    {
        await MopupService.Instance.PopAsync();
    }

    [RelayCommand]
    private void InputUnfocused()
    {
        InputText = InputText.Trim();
    }

    [RelayCommand]
    private async Task OpenLink()
    {
        if (!ValidateForm()) return;

        if (!Uri.TryCreate(CompleteUrl, UriKind.Absolute, out var uri))
        {
            await ShowAlert("Error", "The URL is not valid.");
            return;
        }

        try
        {
            await Browser.Default.OpenAsync(uri, BrowserLaunchMode.External);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error launching browser for URL: {Uri}", uri);
            await ShowAlert("Error", "There was an error trying to launch the default browser.");
        }
    }

    private bool ValidateForm()
    {
        ErrorText = string.Empty;

        if (string.IsNullOrWhiteSpace(InputText))
        {
            ErrorText = "URL cannot be empty";
            return false;
        }

        if (!IsUrlValid)
        {
            ErrorText = "The URL is not valid";
            return false;
        }

        return true;
    }

    private async Task SaveProfile()
    {
        IsBusy = true;

        try
        {
            var result = await _userService.SaveSocialMedia(_platformId, CompleteUrl);
            var snackbarOptions = CreateSnackbarOptions(result);

            // Close window as both true and false are successful states in this case
            if (result.HasValue)
            {
                await ClosePage();
            }

            await _snackbarService.ShowSnackbar(snackbarOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving social media for platform {PlatformId}", _platformId);
            await ShowAlert("Error", "There was an error saving your social media. Please try again later.");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private SnackbarOptions CreateSnackbarOptions(bool? result)
    {
        return result switch
        {
            true => new SnackbarOptions
            {
                ShowPoints = true,
                Points = NewConnectionPoints,
                Message = $"Thanks for connecting your {PlatformName} with SSW Rewards",
                Glyph = TickIcon,
                ActionCompleted = true
            },
            false => new SnackbarOptions
            {
                Message = $"{PlatformName} has been successfully updated",
                Glyph = TickIcon,
                ActionCompleted = true
            },
            null => new SnackbarOptions
            {
                Message = $"Couldn't connect your {PlatformName}, please try again later",
                Glyph = CrossIcon,
                ActionCompleted = false
            }
        };
    }

    private async Task ShowErrorAndClose(string message)
    {
        await ShowAlert("Error", message);
        await ClosePage();
    }

    private async Task ShowAlert(string title, string message)
    {
        await _alertService.ShowAlertAsync(title, message, "OK");
    }
}