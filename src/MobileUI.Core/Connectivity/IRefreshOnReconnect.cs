namespace SSW.Rewards.Mobile.Services;

/// <summary>
/// Implemented by a page's ViewModel to have <c>BaseContentPage</c> refresh it when connectivity
/// returns: straight away while the page is visible, or when it next appears if it was hidden at the time.
/// </summary>
public interface IRefreshOnReconnect
{
    /// <summary>Reloads the page's data from the network. A failure behind cached content should stay quiet.</summary>
    Task RefreshOnReconnectAsync();
}
