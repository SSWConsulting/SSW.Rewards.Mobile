using System.Net;
using FluentAssertions;
using NUnit.Framework;
using SSW.Rewards.Mobile.Common;
using SSW.Rewards.Mobile.Services;

namespace SSW.Rewards.Mobile.Core.UnitTests.ErrorHandling;

public class ApiErrorPresenterTests
{
    private const string Subject = "the activity feed";
    private const string Action = "redeem a reward";

    private sealed record AlertCall(string Title, string Message, string Cancel);

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

    /// <summary>Treats a 401 as handled (the re-login flow), like the app's handler.</summary>
    private sealed class FakeApiExceptionHandler : IApiExceptionHandler
    {
        public int Calls { get; private set; }

        public Task<bool> TryHandleAsync(Exception exception)
        {
            Calls++;
            return Task.FromResult(exception.IsUnauthorized());
        }
    }

    private sealed class FakeConnectivity : IConnectivityService
    {
        public bool IsOnline { get; set; } = true;
        public event EventHandler<bool>? ConnectivityChanged;
        public void Raise() => ConnectivityChanged?.Invoke(this, IsOnline);
    }

    // NUnit shares one fixture instance across tests, so state is rebuilt per test.
    private FakeAlertService _alerts = null!;
    private FakeApiExceptionHandler _api = null!;
    private FakeConnectivity _connectivity = null!;
    private ApiErrorPresenter _presenter = null!;

    [SetUp]
    public void SetUp()
    {
        _alerts = new FakeAlertService();
        _api = new FakeApiExceptionHandler();
        _connectivity = new FakeConnectivity();
        _presenter = new ApiErrorPresenter(_api, _connectivity, _alerts);
    }

    private static readonly Exception NetworkError = new HttpRequestException("network");
    private static readonly Exception Unauthorized = new HttpRequestException("unauthorized", null, HttpStatusCode.Unauthorized);

    private static ListLoadResult Failed(bool hasContent, Exception? error = null) =>
        ListLoadResult.Fail(error ?? NetworkError, hasContent);

    // --- EnsureOnlineAsync ---

    [Test]
    public async Task EnsureOnline_WhenOnline_ReturnsTrueSilently()
    {
        (await _presenter.EnsureOnlineAsync(Action)).Should().BeTrue();
        _alerts.Alerts.Should().BeEmpty();
    }

    [Test]
    public async Task EnsureOnline_WhenOffline_SaysWhyAndReturnsFalse()
    {
        _connectivity.IsOnline = false;

        (await _presenter.EnsureOnlineAsync(Action)).Should().BeFalse();

        _alerts.Alerts.Should().ContainSingle().Which.Should().Be(
            new AlertCall("You're offline", "Connect to the internet to redeem a reward.", "OK"));
    }

    // --- HandleLoadAsync (list loads) ---

    [Test]
    public async Task Load_NoError_DoesNothing()
    {
        await _presenter.HandleLoadAsync(ListLoadResult.Ok(hasContent: true, fromCache: false), userRequestedNewData: false, Subject);

        _alerts.Alerts.Should().BeEmpty();
        _api.Calls.Should().Be(0, "there is nothing to handle when the load succeeded");
    }

    [Test]
    public async Task Load_Unauthorized_GoesToReloginOnly()
    {
        await _presenter.HandleLoadAsync(Failed(hasContent: false, Unauthorized), userRequestedNewData: true, Subject);

        _api.Calls.Should().Be(1);
        _alerts.Alerts.Should().BeEmpty("the re-login flow shows its own alert");
    }

    [Test]
    public async Task Load_BackgroundRefreshBehindCachedContent_StaysSilent()
    {
        _connectivity.IsOnline = false;

        await _presenter.HandleLoadAsync(Failed(hasContent: true), userRequestedNewData: false, Subject);

        _alerts.Alerts.Should().BeEmpty("the banner already says the data is saved");
    }

    [Test]
    public async Task Load_UserRequestedNewData_AlertsEvenWithContent()
    {
        await _presenter.HandleLoadAsync(Failed(hasContent: true), userRequestedNewData: true, Subject);

        _alerts.Alerts.Should().ContainSingle().Which.Should().Be(
            new AlertCall("Something went wrong", "We couldn't load the activity feed. Please try again soon.", "OK"));
    }

    [Test]
    public async Task Load_OfflineWithNothingToShow_UsesOfflineWording()
    {
        _connectivity.IsOnline = false;

        await _presenter.HandleLoadAsync(Failed(hasContent: false), userRequestedNewData: false, Subject);

        _alerts.Alerts.Should().ContainSingle().Which.Should().Be(
            new AlertCall("You're offline", "Connect to the internet to load the activity feed.", "OK"));
    }

    // --- HandleLoadErrorAsync (non-list loads) ---

    [Test]
    public async Task LoadError_Offline_UsesOfflineWording()
    {
        _connectivity.IsOnline = false;

        await _presenter.HandleLoadErrorAsync(NetworkError, "quizzes");

        _alerts.Alerts.Should().ContainSingle().Which.Message.Should().Be("Connect to the internet to load quizzes.");
    }

    [Test]
    public async Task LoadError_Unauthorized_GoesToReloginOnly()
    {
        await _presenter.HandleLoadErrorAsync(Unauthorized, "quizzes");

        _alerts.Alerts.Should().BeEmpty();
    }

    // --- HandleActionErrorAsync ---

    [Test]
    public async Task ActionError_Online_UsesGenericWording()
    {
        await _presenter.HandleActionErrorAsync(new InvalidOperationException("boom"), "submit your quiz");

        _alerts.Alerts.Should().ContainSingle().Which.Should().Be(
            new AlertCall("Something went wrong", "We couldn't submit your quiz. Please try again soon.", "OK"));
    }

    [Test]
    public async Task ActionError_ConnectionDroppedMidRequest_UsesOfflineWording()
    {
        _connectivity.IsOnline = false;

        await _presenter.HandleActionErrorAsync(NetworkError, "submit your quiz");

        _alerts.Alerts.Should().ContainSingle().Which.Should().Be(
            new AlertCall("You're offline", "Connect to the internet to submit your quiz.", "OK"));
    }

    [Test]
    public async Task ActionError_Unauthorized_GoesToReloginOnly()
    {
        await _presenter.HandleActionErrorAsync(Unauthorized, "submit your quiz");

        _api.Calls.Should().Be(1);
        _alerts.Alerts.Should().BeEmpty();
    }
}
