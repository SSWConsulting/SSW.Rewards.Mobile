namespace SSW.Rewards.Mobile.Common;

/// <summary>Outcome of a load. <see cref="Error"/> is null on success or cancellation.</summary>
public readonly record struct ListLoadResult(bool HasContent, bool FromCache, Exception? Error)
{
    public static ListLoadResult Ok(bool hasContent, bool fromCache) => new(hasContent, fromCache, null);
    public static ListLoadResult Fail(Exception error, bool hasContent) => new(hasContent, false, error);
}
