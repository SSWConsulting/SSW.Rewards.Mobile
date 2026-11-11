using FluentAssertions;
using NUnit.Framework;
using SSW.Rewards.Mobile.Services;

namespace SSW.Rewards.Mobile.Core.UnitTests.Caching;

public class CachedItemLoaderTests
{
    private const string Key = "PostDetail_1";

    private sealed record Item(string Value);

    private sealed class FakeCache : IFileCacheService
    {
        public Dictionary<string, object?> Store { get; } = [];
        public long Generation { get; private set; }

        public Task<T?> GetAsync<T>(string cacheKey)
            => Task.FromResult(Store.TryGetValue(cacheKey, out var value) ? (T?)value : default);

        public Task SetAsync<T>(string cacheKey, T value, TimeSpan? expiry = null, long? generation = null)
        {
            if (generation is not { } expected || expected == Generation)
            {
                Store[cacheKey] = value;
            }

            return Task.CompletedTask;
        }

        public Task ResetAsync()
        {
            Generation++;
            Store.Clear();
            return Task.CompletedTask;
        }

        public Task FetchAndRefresh<T>(string cacheKey, Func<Task<T>> fetchCallback, Func<T, bool, object?, Task> dataCallback, object? tag = null)
            => throw new NotSupportedException("Legacy path not under test.");
    }

    // NUnit shares one fixture instance across tests, so state is rebuilt per test.
    private FakeCache _cache = null!;
    private List<(Item Item, bool FromCache)> _shown = null!;

    [SetUp]
    public void SetUp()
    {
        _cache = new FakeCache();
        _shown = [];
    }

    private Task Show(Item item, bool fromCache)
    {
        _shown.Add((item, fromCache));
        return Task.CompletedTask;
    }

    [Test]
    public async Task LoadAsync_WithCachedCopy_ShowsCachedThenFreshAndCachesFresh()
    {
        _cache.Store[Key] = new Item("cached");

        var result = await CachedItemLoader.LoadAsync(_cache, Key, _ => Task.FromResult(new Item("fresh")), Show);

        _shown.Should().Equal((new Item("cached"), true), (new Item("fresh"), false));
        _cache.Store[Key].Should().Be(new Item("fresh"));
        result.Error.Should().BeNull();
        result.FromCache.Should().BeFalse();
    }

    [Test]
    public async Task LoadAsync_OfflineWithCachedCopy_KeepsCachedCopyAndReportsErrorBehindContent()
    {
        _cache.Store[Key] = new Item("cached");

        var result = await CachedItemLoader.LoadAsync<Item>(
            _cache, Key, _ => throw new HttpRequestException("offline"), Show);

        _shown.Should().Equal((new Item("cached"), true));
        result.Error.Should().BeOfType<HttpRequestException>();
        result.HasContent.Should().BeTrue("the caller stays quiet behind the cached copy");
    }

    [Test]
    public async Task LoadAsync_OfflineWithoutCachedCopy_ReportsErrorWithNoContent()
    {
        var result = await CachedItemLoader.LoadAsync<Item>(
            _cache, Key, _ => throw new HttpRequestException("offline"), Show);

        _shown.Should().BeEmpty();
        result.Error.Should().NotBeNull();
        result.HasContent.Should().BeFalse();
    }

    [Test]
    public async Task LoadAsync_CacheResetDuringFetch_DoesNotWriteBack()
    {
        // Logout while the fetch is in flight: the previous user's item must not be cached again.
        var result = await CachedItemLoader.LoadAsync(_cache, Key, async _ =>
        {
            await _cache.ResetAsync();
            return new Item("previous user");
        }, Show);

        _cache.Store.Should().NotContainKey(Key);
        result.Error.Should().BeNull();
    }

    [Test]
    public async Task LoadAsync_Superseded_ShowsNothingMoreAndReportsNoError()
    {
        using var cts = new CancellationTokenSource();

        var result = await CachedItemLoader.LoadAsync<Item>(_cache, Key, _ =>
        {
            cts.Cancel();
            throw new TaskCanceledException();
        }, Show, cts.Token);

        _shown.Should().BeEmpty();
        result.Error.Should().BeNull();
        _cache.Store.Should().BeEmpty();
    }
}
