using System.Net;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using SSW.Rewards.ApiClient.Resilience;

namespace SSW.Rewards.Mobile.Core.UnitTests.Resilience;

public class OfflineFallbackHandlerTests
{
    private const string Url = "https://api.test/api/Leaderboard/GetMobilePaginated?page=0&pageSize=50";

    private sealed class FakeCache : IApiResponseCache
    {
        public Dictionary<string, CachedApiResponse> Store { get; } = [];
        public long Generation { get; set; }
        public int Writes { get; private set; }

        public Task<CachedApiResponse?> GetAsync(string key)
            => Task.FromResult(Store.GetValueOrDefault(key));

        public Task SetAsync(string key, CachedApiResponse response, TimeSpan expiry, long generation)
        {
            if (generation == Generation)
            {
                Writes++;
                Store[key] = response;
            }

            return Task.CompletedTask;
        }

        public Task RemoveAsync(string key)
        {
            Store.Remove(key);
            return Task.CompletedTask;
        }
    }

    /// <summary>The rest of the pipeline: answers each request from a queue.</summary>
    private sealed class FakeInner : HttpMessageHandler
    {
        private readonly Queue<Func<HttpRequestMessage, Task<HttpResponseMessage>>> _responses = new();
        public Action? OnSend { get; set; }

        public FakeInner Returns(HttpStatusCode status, string body = "", string contentType = "application/json")
        {
            _responses.Enqueue(request => Task.FromResult(new HttpResponseMessage(status)
            {
                Content = new StringContent(body, Encoding.UTF8, contentType),
                RequestMessage = request,
            }));
            return this;
        }

        public FakeInner Throws(Exception exception)
        {
            _responses.Enqueue(_ => Task.FromException<HttpResponseMessage>(exception));
            return this;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            OnSend?.Invoke();
            return _responses.Dequeue()(request);
        }
    }

    private static HttpRequestException Offline() => new("No such host is known.");

    private FakeCache _cache = null!;
    private FakeInner _inner = null!;
    private HttpMessageInvoker _client = null!;

    [SetUp]
    public void SetUp()
    {
        _cache = new FakeCache();
        _inner = new FakeInner();
        _client = new HttpMessageInvoker(new OfflineFallbackHandler(_cache) { InnerHandler = _inner });
    }

    [TearDown]
    public void TearDown() => _client.Dispose();

    private Task<HttpResponseMessage> Send(HttpMethod method, string url = Url, CancellationToken ct = default)
        => _client.SendAsync(new HttpRequestMessage(method, url), ct);

    [Test]
    public async Task Get_Succeeds_StoresTheResponse_AndTheCallerCanStillReadIt()
    {
        _inner.Returns(HttpStatusCode.OK, """{"items":[1]}""");

        var response = await Send(HttpMethod.Get);

        (await response.Content.ReadAsStringAsync()).Should().Be("""{"items":[1]}""");
        var stored = _cache.Store[OfflineFallbackHandler.KeyFor(new Uri(Url))];
        Encoding.UTF8.GetString(stored.Body).Should().Be("""{"items":[1]}""");
        stored.ContentType.Should().StartWith("application/json");
    }

    [Test]
    public async Task Get_NetworkFails_AfterASuccess_ReplaysTheStoredResponse()
    {
        _inner.Returns(HttpStatusCode.OK, """{"items":[1]}""").Throws(Offline());
        await Send(HttpMethod.Get);

        var replay = await Send(HttpMethod.Get);

        replay.StatusCode.Should().Be(HttpStatusCode.OK);
        (await replay.Content.ReadAsStringAsync()).Should().Be("""{"items":[1]}""");
        replay.Content.Headers.ContentType!.MediaType.Should().Be("application/json");
        replay.Headers.Contains(OfflineFallbackHandler.ReplayedHeader).Should().BeTrue();
    }

    [Test]
    public async Task Get_NetworkFails_NothingStored_Throws()
    {
        _inner.Throws(Offline());

        var act = () => Send(HttpMethod.Get);

        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [Test]
    public async Task Get_DifferentQuery_IsNotReplayedForAnother()
    {
        _inner.Returns(HttpStatusCode.OK, "page 0").Throws(Offline());
        await Send(HttpMethod.Get, "https://api.test/api/Posts?page=0");

        var act = () => Send(HttpMethod.Get, "https://api.test/api/Posts?page=1");

        await act.Should().ThrowAsync<HttpRequestException>("page 1 was never loaded");
    }

    [Test]
    public async Task Get_ApiReturnsAnError_PassesThrough_AndIsNotStored()
    {
        _inner.Returns(HttpStatusCode.OK, "old").Returns(HttpStatusCode.Unauthorized, "token expired");
        await Send(HttpMethod.Get);

        var response = await Send(HttpMethod.Get);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized, "a 401 must still end the session");
        _cache.Writes.Should().Be(1);
    }

    [Test]
    public async Task Get_ApiReturnsNotFound_DropsTheStoredResponse_SoItIsNotReplayedOffline()
    {
        _inner.Returns(HttpStatusCode.OK, "post 42").Returns(HttpStatusCode.NotFound).Throws(Offline());
        await Send(HttpMethod.Get);

        (await Send(HttpMethod.Get)).StatusCode.Should().Be(HttpStatusCode.NotFound);
        var act = () => Send(HttpMethod.Get);

        _cache.Store.Should().BeEmpty();
        await act.Should().ThrowAsync<HttpRequestException>("a deleted post must not come back offline");
    }

    [Test]
    public async Task Get_HttpErrorAsException_IsNotMaskedByTheStoredResponse()
    {
        _inner.Returns(HttpStatusCode.OK, "old").Throws(new HttpRequestException("forbidden", null, HttpStatusCode.Forbidden));
        await Send(HttpMethod.Get);

        var act = () => Send(HttpMethod.Get);

        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [Test]
    public async Task Get_CancelledByTheCaller_IsNotReplayed()
    {
        using var cts = new CancellationTokenSource();
        _inner.Returns(HttpStatusCode.OK, "old").Throws(Offline());
        await Send(HttpMethod.Get);
        _inner.OnSend = cts.Cancel;

        var act = () => Send(HttpMethod.Get, ct: cts.Token);

        await act.Should().ThrowAsync<HttpRequestException>("a superseded load must not get stale data");
    }

    [Test]
    public async Task Post_IsNeverStoredOrReplayed()
    {
        _inner.Returns(HttpStatusCode.OK, "claimed").Throws(Offline());

        await Send(HttpMethod.Post);
        var act = () => Send(HttpMethod.Post);

        _cache.Writes.Should().Be(0);
        await act.Should().ThrowAsync<HttpRequestException>("writes stay online-only");
    }

    [Test]
    public async Task Get_CacheWipedWhileInFlight_DoesNotStoreThePreviousSessionsResponse()
    {
        _inner.Returns(HttpStatusCode.OK, "previous user");
        _inner.OnSend = () => _cache.Generation++; // sign-out lands mid-request

        await Send(HttpMethod.Get);

        _cache.Store.Should().BeEmpty();
    }
}
