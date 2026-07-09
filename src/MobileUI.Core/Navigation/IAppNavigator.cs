#nullable enable

namespace SSW.Rewards.Mobile.Services;

/// <summary>
/// Semantic, host-independent navigation. Describes destinations by intent rather than
/// by page type or MAUI navigation primitives, so ViewModels stay free of Shell/Page
/// references and the navigation wiring is testable.
/// </summary>
public interface IAppNavigator
{
    /// <summary>Navigates to the current user's own profile.</summary>
    Task GoToMyProfileAsync();

    /// <summary>Navigates to another user's profile.</summary>
    Task GoToUserProfileAsync(int userId);

    /// <summary>Dismisses the current modal page.</summary>
    Task PopModalAsync();
}
