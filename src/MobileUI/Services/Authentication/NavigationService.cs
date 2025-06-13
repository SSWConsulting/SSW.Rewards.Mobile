using Microsoft.Extensions.Logging;

namespace SSW.Rewards.Mobile.Services.Authentication;

public interface INavigationService
{
    void NavigateToLoginPage();
    Task InitializeMainPageAsync();
}

public class NavigationService : INavigationService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<NavigationService> _logger;

    public NavigationService(IServiceProvider serviceProvider, ILogger<NavigationService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public void NavigateToLoginPage()
    {
        App.Current.MainPage = _serviceProvider.GetRequiredService<LoginPage>();
    }

    public async Task InitializeMainPageAsync()
    {
        try
        {
            await App.InitialiseMainPageAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing main page");
            throw;
        }
    }
}
