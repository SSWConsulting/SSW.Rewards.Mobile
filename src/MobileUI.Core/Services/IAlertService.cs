#nullable enable

namespace SSW.Rewards.Mobile.Services;

/// <summary>
/// Host-independent alert surface. Members use only primitive types so Core policy
/// code can prompt the user without referencing MAUI. The MAUI implementation lives
/// in the app project.
/// </summary>
public interface IAlertService
{
    Task DisplayAlertAsync(string title, string message, string cancel);
    Task<bool> DisplayConfirmationAsync(string title, string message, string accept, string cancel);
}
