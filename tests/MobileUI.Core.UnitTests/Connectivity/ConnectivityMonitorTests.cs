using CommunityToolkit.Mvvm.Messaging;
using FluentAssertions;
using Microsoft.Extensions.Time.Testing;
using NUnit.Framework;
using SSW.Rewards.Mobile.Services;

namespace SSW.Rewards.Mobile.Core.UnitTests.Connectivity;

public class ConnectivityMonitorTests
{
    private static readonly TimeSpan Debounce = TimeSpan.FromSeconds(2);

    private sealed class FakeConnectivity : IConnectivityService
    {
        public bool IsOnline { get; private set; } = true;
        public event EventHandler<bool>? ConnectivityChanged;

        public void Set(bool online)
        {
            IsOnline = online;
            ConnectivityChanged?.Invoke(this, online);
        }
    }

    // NUnit shares one fixture instance across tests, so state is rebuilt per test.
    private FakeConnectivity _connectivity = null!;
    private FakeTimeProvider _time = null!;
    private StrongReferenceMessenger _messenger = null!;
    private int _restoredMessages;
    private ConnectivityMonitor _monitor = null!;

    [SetUp]
    public void SetUp()
    {
        _connectivity = new FakeConnectivity();
        _time = new FakeTimeProvider();
        _messenger = new StrongReferenceMessenger();
        _restoredMessages = 0;
        _messenger.Register<ConnectivityRestoredMessage>(this, (_, _) => _restoredMessages++);
        _monitor = Create();
    }

    [TearDown]
    public void TearDown() => _monitor.Dispose();

    // Inline dispatch: the MAUI app passes MainThread.BeginInvokeOnMainThread.
    private ConnectivityMonitor Create() => new(_connectivity, _messenger, action => action(), Debounce, _time);

    /// <summary>Advances the fake clock and lets the debounce continuation run.</summary>
    private async Task AdvanceAsync(TimeSpan by)
    {
        _time.Advance(by);
        await Task.Delay(50);
    }

    [Test]
    public void StartsOffline_WhenDeviceIsOfflineAtLaunch()
    {
        _monitor.Dispose();
        _connectivity.Set(false);

        _monitor = Create();

        _monitor.IsOffline.Should().BeTrue("a cold start in airplane mode shows the banner without waiting");
    }

    [Test]
    public async Task GoingOffline_ShowsBannerOnlyAfterDebounce()
    {
        _connectivity.Set(false);
        await AdvanceAsync(Debounce - TimeSpan.FromMilliseconds(100));
        _monitor.IsOffline.Should().BeFalse();

        await AdvanceAsync(TimeSpan.FromMilliseconds(200));
        _monitor.IsOffline.Should().BeTrue();
    }

    [Test]
    public async Task Reconnect_ClearsBannerAndSendsOneRestoredMessage()
    {
        _connectivity.Set(false);
        await AdvanceAsync(Debounce);

        _connectivity.Set(true);
        await AdvanceAsync(Debounce);

        _monitor.IsOffline.Should().BeFalse();
        _restoredMessages.Should().Be(1);
        _monitor.ReconnectCount.Should().Be(1);
    }

    [Test]
    public async Task Flicker_ShorterThanDebounce_ChangesNothing()
    {
        // Wi-Fi handover: off and back on within the debounce window.
        _connectivity.Set(false);
        await AdvanceAsync(TimeSpan.FromMilliseconds(500));
        _connectivity.Set(true);
        await AdvanceAsync(Debounce);

        _monitor.IsOffline.Should().BeFalse();
        _restoredMessages.Should().Be(0, "nothing was lost, so pages shouldn't refresh");
        _monitor.ReconnectCount.Should().Be(0);
    }

    [Test]
    public async Task FlickerWhileOffline_DoesNotSendRestoredMessage()
    {
        _connectivity.Set(false);
        await AdvanceAsync(Debounce);

        // Briefly online, then offline again before the debounce settles.
        _connectivity.Set(true);
        await AdvanceAsync(TimeSpan.FromMilliseconds(500));
        _connectivity.Set(false);
        await AdvanceAsync(Debounce);

        _monitor.IsOffline.Should().BeTrue();
        _restoredMessages.Should().Be(0);
    }

    [Test]
    public async Task EachReconnect_IncrementsReconnectCount()
    {
        for (var i = 0; i < 2; i++)
        {
            _connectivity.Set(false);
            await AdvanceAsync(Debounce);
            _connectivity.Set(true);
            await AdvanceAsync(Debounce);
        }

        _monitor.ReconnectCount.Should().Be(2, "pages compare it to catch a reconnect they missed while hidden");
        _restoredMessages.Should().Be(2);
    }

    [Test]
    public async Task AfterDispose_IgnoresConnectivityChanges()
    {
        _monitor.Dispose();

        _connectivity.Set(false);
        await AdvanceAsync(Debounce);

        _monitor.IsOffline.Should().BeFalse();
    }
}
