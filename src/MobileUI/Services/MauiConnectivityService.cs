namespace SSW.Rewards.Mobile.Services;

/// <summary>
/// MAUI-backed <see cref="IConnectivityService"/> wrapping <see cref="Connectivity.Current"/>.
/// Forwards MAUI's connectivity change events as a simple online/offline boolean.
/// </summary>
public sealed class MauiConnectivityService : IConnectivityService
{
    private readonly IConnectivity _connectivity;

    public MauiConnectivityService()
        : this(Connectivity.Current)
    {
    }

    public MauiConnectivityService(IConnectivity connectivity)
    {
        _connectivity = connectivity;
        _connectivity.ConnectivityChanged += OnConnectivityChanged;
    }

    public bool IsOnline => _connectivity.NetworkAccess == NetworkAccess.Internet;

    public event EventHandler<bool>? ConnectivityChanged;

    private void OnConnectivityChanged(object? sender, ConnectivityChangedEventArgs e)
        => ConnectivityChanged?.Invoke(this, e.NetworkAccess == NetworkAccess.Internet);
}
