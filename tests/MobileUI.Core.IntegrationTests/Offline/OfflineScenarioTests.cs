using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using SSW.Rewards.ApiClient;
using SSW.Rewards.ApiClient.Services;
using SSW.Rewards.Mobile.Common;
using SSW.Rewards.Mobile.Services;
using SSW.Rewards.Shared.DTOs.ActivityFeed;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace SSW.Rewards.Mobile.Core.IntegrationTests.Offline;

/// <summary>
/// Airplane mode without an airplane: the REAL production chain — typed ApiClient service,
/// the registered resilience pipeline, Akavache-backed cache, PagedListSource — against an
/// in-process WireMock API that we can stop or fault at will.
/// </summary>
public class OfflineScenarioTests
{
    private const string FeedRoute = "/api/ActivityFeed/GetAllActivities";

    /// <summary>Stands in for the auth handler the app registers; auth is not under test.</summary>
    private sealed class PassthroughAuthHandler : DelegatingHandler;

    private WireMockServer _server = null!;
    private ServiceProvider _services = null!;

    [SetUp]
    public void SetUp()
    {
        _server = WireMockServer.Start();
        var collection = new ServiceCollection();
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

    private void StubFeed(params int[] userIds) =>
        _server.Given(Request.Create().WithPath(FeedRoute).UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBodyAsJson(new ActivityFeedViewModel
            {
                Feed = userIds.Select(id => new ActivityFeedItemDto { UserId = id, UserName = $"user-{id}" }),
            }));

    private PagedListSource<ActivityFeedItemDto> CreateFeedSource(string cacheKey) =>
        new(new PagedListSourceOptions<ActivityFeedItemDto>
        {
            FetchPage = async (skip, take, ct) =>
                (await _services.GetRequiredService<IActivityFeedService>().GetAllActivities(take, skip, ct)).Feed.ToList(),
            PageSize = 50,
            Cache = new AkavacheCacheService(NullLogger<AkavacheCacheService>.Instance),
            CacheKey = cacheKey,
        });

    [Test]
    public async Task GoingOffline_ColdStartServesCachedFeed_AndReportsTheError()
    {
        var cacheKey = $"offline-{Guid.NewGuid():N}";
        StubFeed(1, 2, 3);

        // Online session fills the cache through the real HTTP + serialization stack.
        var onlineSession = CreateFeedSource(cacheKey);
        var online = await onlineSession.RefreshAsync();
        online.Error.Should().BeNull();
        onlineSession.Items.Should().HaveCount(3);

        // "Airplane mode": the API is gone. A fresh source = app cold start.
        _server.Stop();
        var offlineSession = CreateFeedSource(cacheKey);
        var offline = await offlineSession.RefreshAsync();

        offline.Error.Should().NotBeNull("the network leg failed");
        offline.HasContent.Should().BeTrue("cached content is on screen");
        offlineSession.Items.Select(i => i.UserId).Should().Equal(1, 2, 3);
        offlineSession.IsShowingCachedData.Should().BeTrue();
    }

    [Test]
    public async Task TransientServerError_OnGet_IsRetriedToSuccess()
    {
        _server.Given(Request.Create().WithPath(FeedRoute).UsingGet())
            .InScenario("flaky").WillSetStateTo("recovered")
            .RespondWith(Response.Create().WithStatusCode(503));
        _server.Given(Request.Create().WithPath(FeedRoute).UsingGet())
            .InScenario("flaky").WhenStateIs("recovered")
            .RespondWith(Response.Create().WithStatusCode(200).WithBodyAsJson(new ActivityFeedViewModel
            {
                Feed = [new ActivityFeedItemDto { UserId = 42 }],
            }));

        var result = await _services.GetRequiredService<IActivityFeedService>().GetAllActivities(50, 0, CancellationToken.None);

        result.Feed.Single().UserId.Should().Be(42, "the resilience pipeline retried the transient 503");
        _server.LogEntries.Count().Should().Be(2, "exactly one retry was needed");
    }

    [Test]
    public async Task ServerError_OnPost_IsNeverRetried()
    {
        const string route = "/api/redeem";
        _server.Given(Request.Create().WithPath(route).UsingPost())
            .RespondWith(Response.Create().WithStatusCode(503));

        var client = _services.GetRequiredService<IHttpClientFactory>().CreateClient(Constants.AuthenticatedClient);
        var response = await client.PostAsync(route, new StringContent("{}"));

        response.IsSuccessStatusCode.Should().BeFalse();
        _server.LogEntries.Count().Should().Be(1, "non-idempotent requests must never be replayed");
    }
}
