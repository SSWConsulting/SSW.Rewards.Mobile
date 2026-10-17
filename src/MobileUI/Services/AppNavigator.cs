using Microsoft.Extensions.DependencyInjection;

namespace SSW.Rewards.Mobile.Services;

/// <summary>
/// MAUI-backed <see cref="IAppNavigator"/>. Owns the page construction (via
/// <see cref="ActivatorUtilities"/>) and Shell navigation that used to live in the
/// ViewModels, translating semantic destinations into concrete page pushes.
/// </summary>
public sealed class AppNavigator : IAppNavigator
{
    private readonly IServiceProvider _serviceProvider;

    public AppNavigator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Task GoToMyProfileAsync()
    {
        var page = ActivatorUtilities.CreateInstance<MyProfilePage>(_serviceProvider);
        return Shell.Current.Navigation.PushAsync(page);
    }

    public Task GoToUserProfileAsync(int userId)
    {
        var page = ActivatorUtilities.CreateInstance<OthersProfilePage>(_serviceProvider, userId);
        return Shell.Current.Navigation.PushAsync(page);
    }

    public Task PopModalAsync() => Shell.Current.Navigation.PopModalAsync();
}
