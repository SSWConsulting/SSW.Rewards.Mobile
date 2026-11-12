namespace SSW.Rewards.Mobile.Services;

/// <summary>Sent by <see cref="ConnectivityMonitor"/> when the device is back online (after the debounce).</summary>
public sealed record ConnectivityRestoredMessage;
