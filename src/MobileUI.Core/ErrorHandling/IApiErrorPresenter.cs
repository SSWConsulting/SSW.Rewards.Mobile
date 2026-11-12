#nullable enable

namespace SSW.Rewards.Mobile.Common;

/// <summary>
/// Turns API failures into the right alert, with the same wording on every page:
/// a 401 goes to the re-login flow, offline gets "You're offline", anything else gets a generic message.
/// </summary>
public interface IApiErrorPresenter
{
    /// <summary>
    /// Guard for online-only actions (writes are never queued). Shows the offline alert and
    /// returns false when the device is offline.
    /// </summary>
    /// <param name="action">What the user tried to do, e.g. "redeem a reward".</param>
    Task<bool> EnsureOnlineAsync(string action);

    /// <summary>Reports a failed list load, staying quiet when cached content is still on screen.</summary>
    /// <param name="result">The load outcome.</param>
    /// <param name="userRequestedNewData">
    /// True when the user asked for different data (e.g. a segment switch), so a failure is reported
    /// even if stale content is visible. False for a background or pull-to-refresh.
    /// </param>
    /// <param name="subject">What was loading, e.g. "the activity feed".</param>
    Task HandleLoadAsync(ListLoadResult result, bool userRequestedNewData, string subject);

    /// <summary>Reports a load that failed with nothing to show instead.</summary>
    Task HandleLoadErrorAsync(Exception error, string subject);

    /// <summary>Reports a failed user action, e.g. "submit your quiz".</summary>
    Task HandleActionErrorAsync(Exception error, string action);
}
