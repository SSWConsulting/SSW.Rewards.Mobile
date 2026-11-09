using System.Net;
using FluentAssertions;
using NUnit.Framework;
using SSW.Rewards.Mobile.Common;
using SSW.Rewards.Mobile.Services;

namespace SSW.Rewards.Mobile.Core.UnitTests.ErrorHandling;

public class OfflineAwareListErrorHandlerTests
{
    private const string Title = "Activity Feed";
    private const string OfflineMessage = "You're offline. The activity feed will load once you're back online.";
    private const string GenericMessage = "There seems to be a problem loading the activity feed. Please try again soon.";

    private sealed record AlertCall(string Title, string Message, string Cancel);

    /// <summary>Records every alert; confirmation is never expected in these tests.</summary>
    private sealed class FakeAlertService : IAlertService
    {
        public List<AlertCall> Alerts { get; } = [];

        public Task DisplayAlertAsync(string title, string message, string cancel)
        {
            Alerts.Add(new AlertCall(title, message, cancel));
            return Task.CompletedTask;
        }

        public Task<bool> DisplayConfirmationAsync(string title, string message, string accept, string cancel)
            => throw new NotSupportedException("Confirmation is not part of this policy.");
    }

    /// <summary>Programmable exception handler: returns a fixed verdict and records calls.</summary>
    private sealed class FakeApiExceptionHandler : IApiExceptionHandler
    {
        private readonly bool _handled;
        public int Calls { get; private set; }
        public Exception? LastException { get; private set; }

        public FakeApiExceptionHandler(bool handled) => _handled = handled;

        public Task<bool> TryHandleAsync(Exception exception)
        {
            Calls++;
            LastException = exception;
            return Task.FromResult(_handled);
        }
    }

    private sealed class FakeConnectivity : IConnectivityService
    {
        public FakeConnectivity(bool isOnline) => IsOnline = isOnline;
        public bool IsOnline { get; set; }
        public event EventHandler<bool>? ConnectivityChanged;
        public void Raise(bool isOnline) => ConnectivityChanged?.Invoke(this, isOnline);
    }

    private static (OfflineAwareListErrorHandler Handler, FakeApiExceptionHandler Api, FakeAlertService Alert) Create(
        bool handled = false, bool isOnline = true)
    {
        var api = new FakeApiExceptionHandler(handled);
        var alert = new FakeAlertService();
        var handler = new OfflineAwareListErrorHandler(api, new FakeConnectivity(isOnline), alert);
        return (handler, api, alert);
    }

    private static ListLoadResult Failed(bool hasContent) =>
        ListLoadResult.Fail(new HttpRequestException("offline"), hasContent);

    private static ListLoadResult Unauthorized(bool hasContent) =>
        ListLoadResult.Fail(new HttpRequestException("unauthorized", null, HttpStatusCode.Unauthorized), hasContent);

    [Test]
    public async Task NoError_DoesNotAlertOrConsultExceptionHandler()
    {
        var (handler, api, alert) = Create();

        await handler.HandleAsync(
            ListLoadResult.Ok(hasContent: true, fromCache: false),
            userRequestedNewData: false, Title, OfflineMessage, GenericMessage);

        alert.Alerts.Should().BeEmpty();
        api.Calls.Should().Be(0, "there is nothing to handle when the load succeeded");
    }

    [Test]
    public async Task HandledByExceptionHandler_DoesNotAlert()
    {
        // A 401 that the handler turns into a re-login must not also show an alert,
        // even for a user-requested load that would otherwise surface.
        var (handler, api, alert) = Create(handled: true);

        await handler.HandleAsync(
            Unauthorized(hasContent: false),
            userRequestedNewData: true, Title, OfflineMessage, GenericMessage);

        api.Calls.Should().Be(1);
        alert.Alerts.Should().BeEmpty();
    }

    [Test]
    public async Task BackgroundRefresh_WithCachedContent_StaysSilent()
    {
        var (handler, _, alert) = Create();

        await handler.HandleAsync(
            Failed(hasContent: true),
            userRequestedNewData: false, Title, OfflineMessage, GenericMessage);

        alert.Alerts.Should().BeEmpty("a failed background refresh behind cached content is silent");
    }

    [Test]
    public async Task BackgroundRefresh_WithNoContent_Alerts()
    {
        var (handler, _, alert) = Create(isOnline: true);

        await handler.HandleAsync(
            Failed(hasContent: false),
            userRequestedNewData: false, Title, OfflineMessage, GenericMessage);

        alert.Alerts.Should().ContainSingle()
            .Which.Should().BeEquivalentTo(new AlertCall(Title, GenericMessage, "OK"));
    }

    [Test]
    public async Task UserRequested_WithCachedContent_StillAlerts()
    {
        // Switching segment is an explicit request for new data: a failure must surface
        // even though the previous segment's items are still on screen.
        var (handler, _, alert) = Create(isOnline: true);

        await handler.HandleAsync(
            Failed(hasContent: true),
            userRequestedNewData: true, Title, OfflineMessage, GenericMessage);

        alert.Alerts.Should().ContainSingle()
            .Which.Message.Should().Be(GenericMessage);
    }

    [Test]
    public async Task Offline_RoutesOfflineMessage()
    {
        var (handler, _, alert) = Create(isOnline: false);

        await handler.HandleAsync(
            Failed(hasContent: false),
            userRequestedNewData: false, Title, OfflineMessage, GenericMessage);

        alert.Alerts.Should().ContainSingle()
            .Which.Message.Should().Be(OfflineMessage);
    }

    [Test]
    public async Task Online_RoutesGenericMessage()
    {
        var (handler, _, alert) = Create(isOnline: true);

        await handler.HandleAsync(
            Failed(hasContent: false),
            userRequestedNewData: true, Title, OfflineMessage, GenericMessage);

        alert.Alerts.Should().ContainSingle()
            .Which.Message.Should().Be(GenericMessage);
    }
}
