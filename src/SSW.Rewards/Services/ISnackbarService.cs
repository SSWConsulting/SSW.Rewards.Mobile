using SSW.Rewards.Mobile.Controls;

namespace SSW.Rewards.Mobile.Services;

public interface ISnackbarService
{
    Task ShowSnackbar(SnackbarOptions options);
}
