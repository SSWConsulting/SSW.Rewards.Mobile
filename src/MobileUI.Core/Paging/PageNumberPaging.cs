namespace SSW.Rewards.Mobile.Common;

/// <summary>
/// Adapts <see cref="PagedListSource{T}"/>'s skip/take requests to APIs that page by page number.
/// </summary>
public static class PageNumberPaging
{
    /// <summary>
    /// Zero-based page index for a skip/take request. <see cref="PagedListSource{T}"/> always skips a
    /// whole number of full pages, since a short page ends the list.
    /// </summary>
    public static int ToPageIndex(int skip, int take)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(skip);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(take);

        if (skip % take != 0)
        {
            throw new ArgumentException($"Skip {skip} is not a multiple of page size {take}.", nameof(skip));
        }

        return skip / take;
    }
}
