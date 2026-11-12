#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

namespace SSW.Rewards.Mobile.Services;

/// <summary>
/// App-wide offline state for the UI: drives the offline banner (<see cref="IsOffline"/>) and tells
/// visible pages to refresh when the connection comes back (<see cref="ConnectivityRestoredMessage"/>).
/// Changes are debounced because connectivity flickers (Wi-Fi handover, captive portals), and a
/// flicker should neither flash the banner nor trigger a burst of refreshes.
/// </summary>
public sealed partial class ConnectivityMonitor : ObservableObject, IDisposable
{
    public static readonly TimeSpan DefaultDebounce = TimeSpan.FromSeconds(2);

    private readonly IConnectivityService _connectivity;
    private readonly IMessenger _messenger;
    private readonly Action<Action> _dispatch;
    private readonly TimeSpan _debounce;
    private readonly TimeProvider _timeProvider;
    private readonly object _gate = new();
    private CancellationTokenSource? _pending;

    /// <param name="connectivity">Raw connectivity source.</param>
    /// <param name="messenger">Where <see cref="ConnectivityRestoredMessage"/> is sent.</param>
    /// <param name="dispatch">Runs state changes on the UI thread (bindings and message handlers touch UI).</param>
    /// <param name="debounce">How long a new state must hold before it counts; defaults to <see cref="DefaultDebounce"/>.</param>
    /// <param name="timeProvider">Clock for the debounce; tests pass a fake one.</param>
    public ConnectivityMonitor(
        IConnectivityService connectivity,
        IMessenger messenger,
        Action<Action> dispatch,
        TimeSpan? debounce = null,
        TimeProvider? timeProvider = null)
    {
        _connectivity = connectivity;
        _messenger = messenger;
        _dispatch = dispatch;
        _debounce = debounce ?? DefaultDebounce;
        _timeProvider = timeProvider ?? TimeProvider.System;

        // No debounce at startup: a cold start in airplane mode shows the banner straight away.
        IsOffline = !connectivity.IsOnline;
        _connectivity.ConnectivityChanged += OnConnectivityChanged;
    }

    /// <summary>True while the device has been offline for longer than the debounce.</summary>
    [ObservableProperty]
    public partial bool IsOffline { get; private set; }

    /// <summary>
    /// Increments on every (debounced) reconnect. A page records it when it hides and compares when it
    /// reappears, so it can refresh if it missed a reconnect while it wasn't listening.
    /// </summary>
    public int ReconnectCount { get; private set; }

    private void OnConnectivityChanged(object? sender, bool isOnline)
    {
        CancellationToken token;
        lock (_gate)
        {
            _pending?.Cancel();
            _pending?.Dispose();
            _pending = new CancellationTokenSource();
            token = _pending.Token;
        }

        _ = SettleAsync(token);
    }

    private async Task SettleAsync(CancellationToken token)
    {
        try
        {
            await Task.Delay(_debounce, _timeProvider, token);
        }
        catch (OperationCanceledException)
        {
            // A newer change restarted the debounce.
            return;
        }

        // Read the state now rather than trusting the event payload: it's what held through the debounce.
        var offline = !_connectivity.IsOnline;
        _dispatch(() => Apply(offline));
    }

    private void Apply(bool offline)
    {
        if (offline == IsOffline)
        {
            return;
        }

        IsOffline = offline;
        if (!offline)
        {
            ReconnectCount++;
            _messenger.Send(new ConnectivityRestoredMessage());
        }
    }

    public void Dispose()
    {
        _connectivity.ConnectivityChanged -= OnConnectivityChanged;
        lock (_gate)
        {
            _pending?.Cancel();
            _pending?.Dispose();
            _pending = null;
        }
    }
}
