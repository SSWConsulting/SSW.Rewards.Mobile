using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using SSW.Rewards.Mobile.Services;

namespace SSW.Rewards.Mobile.Core.IntegrationTests.Caching;

/// <summary>Real Akavache SQLite on the host — no mocks, no emulator.</summary>
public class AkavacheCacheServiceTests
{
    private sealed record Payload(int Id, string Name);

    private readonly AkavacheCacheService _cache = new(NullLogger<AkavacheCacheService>.Instance);

    private static string NewKey() => $"it-{Guid.NewGuid():N}";

    [Test]
    public async Task RoundTrip_SurvivesRealSerialization()
    {
        var key = NewKey();

        await _cache.SetAsync(key, new List<Payload> { new(1, "a"), new(2, "b") });
        var read = await _cache.GetAsync<List<Payload>>(key);

        read.Should().Equal(new Payload(1, "a"), new Payload(2, "b"));
    }

    [Test]
    public async Task Get_MissingKey_ReturnsDefault()
    {
        (await _cache.GetAsync<Payload>(NewKey())).Should().BeNull();
    }

    [Test]
    public async Task Get_TypeMismatch_ReturnsDefault_NeverThrows()
    {
        var key = NewKey();
        await _cache.SetAsync(key, "just a string");

        var act = async () => await _cache.GetAsync<List<Payload>>(key);

        (await act.Should().NotThrowAsync()).Which.Should().BeNull();
    }

    [Test]
    public async Task Expiry_EntryDisappearsAfterTtl()
    {
        var key = NewKey();
        await _cache.SetAsync(key, new Payload(1, "short-lived"), expiry: TimeSpan.FromMilliseconds(250));

        (await _cache.GetAsync<Payload>(key)).Should().NotBeNull("not expired yet");
        await Task.Delay(800);
        (await _cache.GetAsync<Payload>(key)).Should().BeNull("the TTL elapsed");
    }

    [Test]
    public async Task Reset_WipesEverything()
    {
        var (k1, k2) = (NewKey(), NewKey());
        await _cache.SetAsync(k1, new Payload(1, "x"));
        await _cache.SetAsync(k2, new Payload(2, "y"));

        await _cache.ResetAsync();

        (await _cache.GetAsync<Payload>(k1)).Should().BeNull();
        (await _cache.GetAsync<Payload>(k2)).Should().BeNull();
    }

    [Test]
    public async Task FetchAndRefresh_CachedThenFresh_LegacySemantics()
    {
        var key = NewKey();
        await _cache.SetAsync(key, new Payload(1, "cached"));
        var calls = new List<(string name, bool isCached)>();

        await _cache.FetchAndRefresh(key,
            () => Task.FromResult(new Payload(2, "fresh")),
            (value, isCached, _) => { calls.Add((value.Name, isCached)); return Task.CompletedTask; });

        calls.Should().Equal(("cached", true), ("fresh", false));
        (await _cache.GetAsync<Payload>(key))!.Name.Should().Be("fresh", "the fresh value replaces the cached one");
    }

    [Test]
    public async Task FetchAndRefresh_FetchFails_StillServesCache_ThenRethrows()
    {
        var key = NewKey();
        await _cache.SetAsync(key, new Payload(1, "cached"));
        var calls = new List<bool>();

        var act = async () => await _cache.FetchAndRefresh<Payload>(key,
            () => throw new HttpRequestException("offline"),
            (_, isCached, _) => { calls.Add(isCached); return Task.CompletedTask; });

        await act.Should().ThrowAsync<HttpRequestException>();
        calls.Should().Equal(true);
    }
}
