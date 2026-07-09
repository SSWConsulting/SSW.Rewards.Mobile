#nullable enable

using SSW.Rewards.Mobile.Services;

namespace SSW.Rewards.Mobile.Common;

/// <summary>
/// Decides whether a failed list load should surface to the user, and with which message.
/// Encapsulates the offline-aware alert policy that used to live inline in the ViewModels:
/// let the API exception handler run first (e.g. 401 → re-login), stay silent when a
/// background refresh fails behind cached content, and otherwise show an offline- or
/// generic-worded alert depending on connectivity.
/// </summary>
public sealed class OfflineAwareListErrorHandler
{
    private readonly IApiExceptionHandler _apiExceptionHandler;
    private readonly IConnectivityService _connectivity;
    private readonly IAlertService _alertService;

    public OfflineAwareListErrorHandler(
        IApiExceptionHandler apiExceptionHandler,
        IConnectivityService connectivity,
        IAlertService alertService)
    {
        _apiExceptionHandler = apiExceptionHandler;
        _connectivity = connectivity;
        _alertService = alertService;
    }

    /// <summary>
    /// Applies the alert policy for a completed load.
    /// </summary>
    /// <param name="result">The load outcome.</param>
    /// <param name="userRequestedNewData">
    /// True when the user explicitly asked for different data (e.g. a segment switch), so a
    /// failure must be reported even if stale content is still visible. False for a background
    /// or pull-to-refresh where cached content can silently remain.
    /// </param>
    /// <param name="subjectTitle">Alert title (e.g. the feature name).</param>
    /// <param name="offlineMessage">Message shown when the device is offline.</param>
    /// <param name="genericMessage">Message shown when online but the load still failed.</param>
    public async Task HandleAsync(
        ListLoadResult result,
        bool userRequestedNewData,
        string subjectTitle,
        string offlineMessage,
        string genericMessage)
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

        var message = _connectivity.IsOnline ? genericMessage : offlineMessage;
        await _alertService.DisplayAlertAsync(subjectTitle, message, "OK");
    }
}
