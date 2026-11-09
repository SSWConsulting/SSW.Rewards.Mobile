using System.Reactive.Linq;
using Akavache;
using Akavache.Sqlite3;
using Akavache.SystemTextJson;
using NUnit.Framework;
using Splat.Builder;

namespace SSW.Rewards.Mobile.Core.IntegrationTests;

/// <summary>
/// Initialises a real Akavache SQLite database once for the whole test assembly —
/// no emulator, the host's SQLite native is used. A unique application name
/// isolates each run's store.
/// </summary>
[SetUpFixture]
public class AkavacheTestSetup
{
    [OneTimeSetUp]
    public void Init()
    {
        AppBuilder.CreateSplatBuilder()
            .WithAkavacheCacheDatabase<SystemJsonSerializer>(
                cache => cache.WithSqliteProvider()
                              .UseForcedDateTimeKind(DateTimeKind.Utc)
                              .WithSqliteDefaults(),
                $"SSW.Rewards.CoreTests-{Guid.NewGuid():N}");
    }

    [OneTimeTearDown]
    public async Task Teardown()
    {
        try
        {
            await CacheDatabase.LocalMachine.InvalidateAll().FirstAsync();
            await CacheDatabase.Shutdown().FirstAsync();
        }
        catch
        {
            // Best-effort cleanup; a failed teardown must not fail the run.
        }
    }
}
