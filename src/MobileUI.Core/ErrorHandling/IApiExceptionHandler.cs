#nullable enable

namespace SSW.Rewards.Mobile.Services;

/// <summary>
/// Host-independent hook for turning an API exception into a side effect (e.g. a 401 →
/// re-login redirect). Returning true means the exception was fully handled and the
/// caller should stay silent.
/// </summary>
public interface IApiExceptionHandler
{
    /// <summary>
    /// Attempts to handle <paramref name="exception"/>.
    /// </summary>
    /// <returns>True when handled (no further alerting needed); otherwise false.</returns>
    Task<bool> TryHandleAsync(Exception exception);
}
