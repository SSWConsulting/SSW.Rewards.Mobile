#nullable enable

namespace SSW.Rewards.Mobile.Services;

/// <summary>
/// Host-independent view of network connectivity. Lets Core policy code (e.g. the
/// offline banner / reconnect refresh) reason about online/offline without touching
/// MAUI's <c>Connectivity.Current</c>.
/// </summary>
public interface IConnectivityService
{
    /// <summary>True when the device currently has internet access.</summary>
    bool IsOnline { get; }

    /// <summary>Raised when connectivity changes. The payload is the new online state (true = online).</summary>
    event EventHandler<bool>? ConnectivityChanged;
}
