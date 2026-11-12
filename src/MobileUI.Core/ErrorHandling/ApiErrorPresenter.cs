#nullable enable

using SSW.Rewards.Mobile.Services;

namespace SSW.Rewards.Mobile.Common;

/// <inheritdoc />
public sealed class ApiErrorPresenter : IApiErrorPresenter
{
    private readonly IApiExceptionHandler _apiExceptionHandler;
    private readonly IConnectivityService _connectivity;
    private readonly IAlertService _alertService;

    public ApiErrorPresenter(
        IApiExceptionHandler apiExceptionHandler,
        IConnectivityService connectivity,
        IAlertService alertService)
    {
        _apiExceptionHandler = apiExceptionHandler;
        _connectivity = connectivity;
        _alertService = alertService;
    }

    public async Task<bool> EnsureOnlineAsync(string action)
    {
        if (_connectivity.IsOnline)
        {
            return true;
        }

        await _alertService.DisplayAlertAsync(UserMessages.OfflineTitle, UserMessages.OfflineAction(action), "OK");
        return false;
    }

    public async Task HandleLoadAsync(ListLoadResult result, bool userRequestedNewData, string subject)
    {
        if (result.Error is null)
        {
            return;
        }

        if (await _apiExceptionHandler.TryHandleAsync(result.Error))
        {
            return;
        }

        // A failed background refresh behind cached content stays quiet.
        if (!userRequestedNewData && result.HasContent)
        {
            return;
        }

        await ShowLoadErrorAsync(subject);
    }

    public async Task HandleLoadErrorAsync(Exception error, string subject)
    {
        if (await _apiExceptionHandler.TryHandleAsync(error))
        {
            return;
        }

        await ShowLoadErrorAsync(subject);
    }

    public async Task HandleActionErrorAsync(Exception error, string action)
    {
        if (await _apiExceptionHandler.TryHandleAsync(error))
        {
            return;
        }

        if (_connectivity.IsOnline)
        {
            await _alertService.DisplayAlertAsync(UserMessages.ErrorTitle, UserMessages.ErrorAction(action), "OK");
        }
        else
        {
            await _alertService.DisplayAlertAsync(UserMessages.OfflineTitle, UserMessages.OfflineAction(action), "OK");
        }
    }

    private Task ShowLoadErrorAsync(string subject) => _connectivity.IsOnline
        ? _alertService.DisplayAlertAsync(UserMessages.ErrorTitle, UserMessages.ErrorLoad(subject), "OK")
        : _alertService.DisplayAlertAsync(UserMessages.OfflineTitle, UserMessages.OfflineLoad(subject), "OK");
}
