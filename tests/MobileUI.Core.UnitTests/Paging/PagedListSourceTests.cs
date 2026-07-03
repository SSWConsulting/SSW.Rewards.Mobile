using FluentAssertions;
using NUnit.Framework;
using SSW.Rewards.Mobile.Common;
using SSW.Rewards.Mobile.Services;

namespace SSW.Rewards.Mobile.Core.UnitTests.Paging;

public class PagedListSourceTests
{
    private sealed class Item
    {
        public int Id { get; init; }
        public string? Display { get; set; }
    }

    private sealed class FakeCache : IFileCacheService
    {
        public Dictionary<string, object?> Store { get; } = [];
        public int Reads { get; private set; }
        public int Writes { get; private set; }

        public Task<T?> GetAsync<T>(string cacheKey)
        {
            Reads++;
            return Task.FromResult(Store.TryGetValue(cacheKey, out var value) ? (T?)value : default);
        }

        public Task SetAsync<T>(string cacheKey, T value, TimeSpan? expiry = null)
        {
            Writes++;
            Store[cacheKey] = value;
            return Task.CompletedTask;
        }

        public Task FetchAndRefresh<T>(string cacheKey, Func<Task<T>> fetchCallback, Func<T, bool, object?, Task> dataCallback, object? tag = null)
            => throw new NotSupportedException("Legacy path not under test.");
    }

    /// <summary>Programmable fetch: enqueue pages or exceptions; records every requested skip.</summary>
    private sealed class FakeFetch
    {
        private readonly Queue<Func<Task<List<Item>>>> _responses = new();
        public List<int> RequestedSkips { get; } = [];

        public FakeFetch Returns(params int[] ids)
        {
            _responses.Enqueue(() => Task.FromResult(ids.Select(i => new Item { Id = i }).ToList()));
            return this;
        }

        public FakeFetch Throws(Exception? ex = null)
        {
            _responses.Enqueue(() => Task.FromException<List<Item>>(ex ?? new HttpRequestException("offline")));
            return this;
        }

        public FakeFetch Pending(out TaskCompletionSource<List<Item>> tcs)
        {
            var source = new TaskCompletionSource<List<Item>>();
            _responses.Enqueue(() => source.Task);
            tcs = source;
            return this;
        }

        public Task<List<Item>> Fetch(int skip, int take, CancellationToken ct)
        {
            RequestedSkips.Add(skip);
            return _responses.Count > 0 ? _responses.Dequeue()() : Task.FromResult(new List<Item>());
        }
    }

    private static List<Item> CachedItems(params int[] ids) => ids.Select(i => new Item { Id = i }).ToList();

    private static PagedListSource<Item> CreateSource(
        FakeFetch fetch, FakeCache? cache = null, int pageSize = 2,
        Action<Item>? prepare = null, Func<Item, Item, bool>? areSame = null, Func<bool>? shouldUseCache = null)
        => new(new PagedListSourceOptions<Item>
        {
            FetchPage = fetch.Fetch,
            PageSize = pageSize,
            PrepareItem = prepare,
            AreSame = areSame,
            Cache = cache,
            CacheKey = cache is null ? null : "test-key",
            ShouldUseCache = shouldUseCache,
        });

    [Test]
    public void Ctor_CacheWithoutKey_Throws()
    {
        var act = () => new PagedListSource<Item>(new PagedListSourceOptions<Item>
        {
            FetchPage = (_, _, _) => Task.FromResult(new List<Item>()),
            Cache = new FakeCache(),
        });

        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public async Task Refresh_NoCache_ShowsFreshPage()
    {
        var fetch = new FakeFetch().Returns(1, 2);
        var source = CreateSource(fetch);

        var result = await source.RefreshAsync();

        result.Error.Should().BeNull();
        result.HasContent.Should().BeTrue();
        source.Items.Select(i => i.Id).Should().Equal(1, 2);
        source.IsShowingCachedData.Should().BeFalse();
    }

    [Test]
    public async Task Refresh_WithCache_WritesFreshFirstPageToCache()
    {
        var cache = new FakeCache();
        var fetch = new FakeFetch().Returns(1, 2);
        var source = CreateSource(fetch, cache);

        await source.RefreshAsync();

        cache.Writes.Should().Be(1);
        cache.Store["test-key"].Should().BeOfType<List<Item>>()
            .Which.Select(i => i.Id).Should().Equal(1, 2);
    }

    [Test]
    public async Task Refresh_NetworkFails_WithCachedData_KeepsCacheAndReportsError()
    {
        var cache = new FakeCache();
        cache.Store["test-key"] = CachedItems(7, 8);
        var fetch = new FakeFetch().Throws();
        var source = CreateSource(fetch, cache);

        var result = await source.RefreshAsync();

        result.Error.Should().BeOfType<HttpRequestException>();
        result.HasContent.Should().BeTrue("cached items are on screen");
        source.Items.Select(i => i.Id).Should().Equal(7, 8);
        source.IsShowingCachedData.Should().BeTrue();
    }

    [Test]
    public async Task Refresh_NetworkFails_NoCache_ReportsNothingToShow()
    {
        var fetch = new FakeFetch().Throws();
        var source = CreateSource(fetch, new FakeCache());

        var result = await source.RefreshAsync();

        result.Error.Should().NotBeNull();
        result.HasContent.Should().BeFalse();
        source.Items.Should().BeEmpty();
    }

    [Test]
    public async Task Refresh_SecondTime_DoesNotReReadCache()
    {
        var cache = new FakeCache();
        cache.Store["test-key"] = CachedItems(7);
        var fetch = new FakeFetch().Returns(1, 2).Returns(3, 4);
        var source = CreateSource(fetch, cache);

        await source.RefreshAsync();
        var readsAfterFirst = cache.Reads;
        await source.RefreshAsync();

        cache.Reads.Should().Be(readsAfterFirst, "cache is a cold-start affordance, not a data source");
        source.Items.Select(i => i.Id).Should().Equal(3, 4);
    }

    [Test]
    public async Task Refresh_ShouldUseCacheFalse_SkipsCacheEntirely()
    {
        var cache = new FakeCache();
        cache.Store["test-key"] = CachedItems(7);
        var fetch = new FakeFetch().Returns(1);
        var source = CreateSource(fetch, cache, shouldUseCache: () => false);

        await source.RefreshAsync();

        cache.Reads.Should().Be(0);
        cache.Writes.Should().Be(0);
        source.Items.Select(i => i.Id).Should().Equal(1);
    }

    [Test]
    public async Task LoadMore_AppendsNextPage_AdvancingSkip()
    {
        var fetch = new FakeFetch().Returns(1, 2).Returns(3, 4);
        var source = CreateSource(fetch);

        await source.RefreshAsync();
        var result = await source.LoadMoreAsync();

        result.Error.Should().BeNull();
        source.Items.Select(i => i.Id).Should().Equal(1, 2, 3, 4);
        fetch.RequestedSkips.Should().Equal(0, 2);
    }

    [Test]
    public async Task LoadMore_AfterShortPage_IsNoOp()
    {
        var fetch = new FakeFetch().Returns(1, 2).Returns(3); // short page = end of list
        var source = CreateSource(fetch);

        await source.RefreshAsync();
        await source.LoadMoreAsync();
        await source.LoadMoreAsync(); // must not fetch again

        fetch.RequestedSkips.Should().Equal(0, 2);
        source.Items.Select(i => i.Id).Should().Equal(1, 2, 3);
    }

    [Test]
    public async Task LoadMore_Failure_DoesNotMarkEnd_SamePageRetries()
    {
        var fetch = new FakeFetch().Returns(1, 2).Throws().Returns(3, 4);
        var source = CreateSource(fetch);

        await source.RefreshAsync();
        var failed = await source.LoadMoreAsync();
        var retried = await source.LoadMoreAsync();

        failed.Error.Should().NotBeNull();
        retried.Error.Should().BeNull();
        fetch.RequestedSkips.Should().Equal(new[] { 0, 2, 2 }, "a failed page must be retryable at the same offset");
        source.Items.Select(i => i.Id).Should().Equal(1, 2, 3, 4);
    }

    [Test]
    public async Task LoadMore_WhileShowingCacheOnly_DoesNotFetch()
    {
        var cache = new FakeCache();
        cache.Store["test-key"] = CachedItems(7, 8);
        var fetch = new FakeFetch().Throws(); // refresh's network leg fails → cache-only display
        var source = CreateSource(fetch, cache);

        await source.RefreshAsync();
        await source.LoadMoreAsync();

        fetch.RequestedSkips.Should().Equal(new[] { 0 }, "appending after cache-only display could duplicate page 1");
        source.Items.Select(i => i.Id).Should().Equal(7, 8);
    }

    [Test]
    public async Task PrepareItem_RunsForCachedAndFreshItems()
    {
        var cache = new FakeCache();
        cache.Store["test-key"] = CachedItems(7);
        var fetch = new FakeFetch().Returns(1);
        var prepared = new List<int>();
        var source = CreateSource(fetch, cache, prepare: i => { i.Display = $"#{i.Id}"; prepared.Add(i.Id); });

        await source.RefreshAsync();

        prepared.Should().Equal(7, 1);
        source.Items.Single().Display.Should().Be("#1");
    }

    [Test]
    public async Task AreSame_IdenticalCacheAndNetwork_DoesNotRedrawCollection()
    {
        var cache = new FakeCache();
        cache.Store["test-key"] = CachedItems(1, 2);
        var fetch = new FakeFetch().Pending(out var fresh);
        var source = CreateSource(fetch, cache, areSame: (a, b) => a.Id == b.Id);

        var refresh = source.RefreshAsync();
        source.Items.Should().HaveCount(2, "the cache leg completes before the network leg");
        source.IsShowingCachedData.Should().BeTrue();

        var changes = 0;
        source.Items.CollectionChanged += (_, _) => changes++;
        fresh.SetResult(CachedItems(1, 2)); // identical fresh page arrives
        await refresh;

        changes.Should().Be(0, "identical data must not redraw the collection");
        source.IsShowingCachedData.Should().BeFalse("the fresh fetch still completed");
    }

    [Test]
    public async Task Refresh_WhileFirstFetchPending_SecondWins()
    {
        var fetch = new FakeFetch().Pending(out var slowFirst).Returns(3, 4);
        var source = CreateSource(fetch);

        var first = source.RefreshAsync();
        var second = await source.RefreshAsync();
        slowFirst.SetResult(CachedItems(1, 2)); // stale response arrives late
        var firstResult = await first;

        second.Error.Should().BeNull();
        source.Items.Select(i => i.Id).Should().Equal(new[] { 3, 4 }, "the superseded load must not clobber the newer one");
        firstResult.Error.Should().BeNull("cancellation is not an error");
    }
}
