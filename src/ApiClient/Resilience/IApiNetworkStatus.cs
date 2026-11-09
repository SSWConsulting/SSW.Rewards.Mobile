namespace SSW.Rewards.ApiClient.Resilience;

/// <summary>
/// Tells the API client whether the device has a network. Register one so failed requests
/// aren't retried while offline, where every retry fails the same way and only delays the error.
/// Without a registration the client assumes it is online.
/// </summary>
public interface IApiNetworkStatus
{
    bool IsOnline { get; }
}
