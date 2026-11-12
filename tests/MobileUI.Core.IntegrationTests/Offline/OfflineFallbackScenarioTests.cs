using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using SSW.Rewards.ApiClient;
using SSW.Rewards.ApiClient.Resilience;
using SSW.Rewards.ApiClient.Services;
using SSW.Rewards.Mobile.Common;
using SSW.Rewards.Mobile.Services;
using SSW.Rewards.Shared.DTOs.ActivityFeed;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace SSW.Rewards.Mobile.Core.IntegrationTests.Offline;

/// <summary>
/// Caching on by default: the API client registered the way the app registers it (with an
/// <see cref="IApiResponseCache"/> over the Akavache cache), against a WireMock API we can stop.
/// No page-level cache is involved, so this is what a new page gets for free.
/// </summary>
public class OfflineFallbackScenarioTests
{
    private const string FeedRoute = "/api/ActivityFeed/GetAllActivities";

    private sealed class PassthroughAuthHandler : DelegatingHandler;

    private sealed class FakeNetworkStatus : IApiNetworkStatus
    {
        public bool IsOnline { get; set; } = true;
    }

    /// <summary>The app's adapter (FileCacheApiResponseCache) lives in the MAUI project; same three lines.</summary>
    private sealed class AppResponseCache(IFileCacheService cache) : IApiResponseCache
    {
        public long Generation => cache.Generation;
        public Task<CachedApiResponse?> GetAsync(string key) => cache.GetAsync<CachedApiResponse>(key);
        public Task SetAsync(string key, CachedApiResponse response, TimeSpan expiry, long generation)
            => cache.SetAsync(key, response, expiry, generation);
        public Task RemoveAsync(string key) => cache.RemoveAsync<CachedApiResponse>(key);
    }

    private WireMockServer _server = null!;
    private ServiceProvider _services = null!;
    private FakeNetworkStatus _network = null!;
    private AkavacheCacheService _cache = null!;

    [SetUp]
    public void SetUp()
    {
        _server = WireMockServer.Start();
        _network = new FakeNetworkStatus();
        _cache = new AkavacheCacheService(NullLogger<AkavacheCacheService>.Instance);

        var collection = new ServiceCollection();
        collection.AddSingleton<IApiNetworkStatus>(_network);
        collection.AddSingleton<IApiResponseCache>(new AppResponseCache(_cache));
        collection.AddTransient<PassthroughAuthHandler>();
        collection.AddApiClientServices<PassthroughAuthHandler>(_server.Url!);
        _services = collection.BuildServiceProvider();
    }

    [TearDown]
    public void TearDown()
    {
        _server.Stop();
        _server.Dispose();
        _services.Dispose();
    }

    private IActivityFeedService Feed => _services.GetRequiredService<IActivityFeedService>();

    private void StubFeed(params int[] userIds) =>
        _server.Given(Request.Create().WithPath(FeedRoute).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBodyAsJson(new ActivityFeedViewModel
            {
                Feed = userIds.Select(id => new ActivityFeedItemDto { UserId = id, UserName = $"user-{id}" }),
            }));

    private void GoOffline()
    {
        _network.IsOnline = false;
        _server.Stop();
    }

    [Test]
    public async Task LoadedOnce_ThenOffline_TheTypedServiceReturnsTheLastResponse()
    {
        StubFeed(1, 2, 3);
        await Feed.GetAllActivities(50, 0, CancellationToken.None);

        GoOffline();
        var offline = await Feed.GetAllActivities(50, 0, CancellationToken.None);

        offline.Feed.Select(i => i.UserId).Should().Equal(1, 2, 3);
    }

    [Test]
    public async Task PageWithoutItsOwnCache_ShowsTheLastResponseOffline_WithoutAnError()
    {
        StubFeed(4, 5);
        PagedListSource<ActivityFeedItemDto> NewPage() => new(new PagedListSourceOptions<ActivityFeedItemDto>
        {
            FetchPage = async (skip, take, ct) => (await Feed.GetAllActivities(take, skip, ct)).Feed.ToList(),
        });
        await NewPage().RefreshAsync();

        GoOffline();
        var coldStart = NewPage();
        var result = await coldStart.RefreshAsync();

        result.Error.Should().BeNull("the replayed response is a normal response to the page");
        coldStart.Items.Select(i => i.UserId).Should().Equal(4, 5);
    }

    [Test]
    public async Task NeverLoaded_Offline_StillFails()
    {
        StubFeed(1);
        GoOffline();

        var act = () => Feed.GetAllActivities(50, 0, CancellationToken.None);

        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [Test]
    public async Task AnotherPage_NeverLoaded_Offline_StillFails()
    {
        StubFeed(1, 2);
        await Feed.GetAllActivities(2, 0, CancellationToken.None);

        GoOffline();
        var act = () => Feed.GetAllActivities(2, 2, CancellationToken.None);

        await act.Should().ThrowAsync<HttpRequestException>("only page 1 was stored");
    }

    [Test]
    public async Task SignedOut_TheReplayIsWiped()
    {
        StubFeed(1, 2, 3);
        await Feed.GetAllActivities(50, 0, CancellationToken.None);

        await _cache.ResetAsync();
        GoOffline();
        var act = () => Feed.GetAllActivities(50, 0, CancellationToken.None);

        await act.Should().ThrowAsync<HttpRequestException>("the previous user's data must not outlive the session");
    }

    [Test]
    public async Task ExpiredSession_IsNotHiddenByTheReplay()
    {
        StubFeed(1, 2, 3);
        await Feed.GetAllActivities(50, 0, CancellationToken.None);

        _server.Reset();
        _server.Given(Request.Create().WithPath(FeedRoute).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(401).WithBody("token expired"));
        var act = () => Feed.GetAllActivities(50, 0, CancellationToken.None);

        (await act.Should().ThrowAsync<HttpRequestException>())
            .Which.IsUnauthorized().Should().BeTrue("a 401 must still send the user to login");
    }
}
