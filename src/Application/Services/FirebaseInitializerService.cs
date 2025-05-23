using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Options;

namespace SSW.Rewards.Application.Services;

public interface IFirebaseInitializerService
{
    void Initialize();
}

internal class FirebaseInitializerService : IFirebaseInitializerService
{
    private readonly Lazy<FirebaseApp> _lazyFirebaseApp;

    public FirebaseInitializerService(IOptions<FirebaseNotificationServiceOptions> options)
    {
        _lazyFirebaseApp = new Lazy<FirebaseApp>(() =>
        {
            if (FirebaseApp.DefaultInstance == null)
            {
                return FirebaseApp.Create(new AppOptions()
                {
                    Credential = GoogleCredential.FromJson(options.Value.FirebaseCredentials)
                });
            }
            return FirebaseApp.DefaultInstance; // Return the existing instance
        });
    }

    public void Initialize()
    {
        _ = _lazyFirebaseApp.Value;
    }
}
