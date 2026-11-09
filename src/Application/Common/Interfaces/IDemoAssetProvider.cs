namespace SSW.Rewards.Application.Common.Interfaces;

/// <summary>
/// Resolves a demo-seed asset key (e.g. an avatar or reward image) to a publicly
/// reachable URI. Returns null when the asset is unavailable — seeding proceeds
/// without the image rather than failing.
/// </summary>
public interface IDemoAssetProvider
{
    Task<string?> GetAssetUriAsync(string key, CancellationToken cancellationToken);
}
