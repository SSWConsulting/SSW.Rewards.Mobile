namespace SSW.Rewards.ApiClient.Paging;

/// <summary>
/// Adapts skip/take requests (the mobile app's <c>PagedListSource</c>) to APIs that page by page number.
/// </summary>
public static class PageNumberPaging
{
    /// <summary>
    /// Zero-based page index for a skip/take request. <c>PagedListSource</c> always skips a
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
