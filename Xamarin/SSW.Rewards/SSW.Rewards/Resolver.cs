using IdentityModel.OidcClient.Browser;
using SSW.Rewards.Helpers;
using SSW.Rewards.Services;
using SSW.Rewards.ViewModels;
using System.Linq;
using System.Reflection;
using TinyIoC;
using Xamarin.Forms;

namespace SSW.Rewards
{
    public static class Resolver
    {
        public static T Resolve<T>()
        {
            var typeToResolve = typeof(T);

            var resolvedType = Container.Resolve(typeToResolve);

            return (T)resolvedType;
        }

        public static AppShell ResolveShell(bool isStaff)
        {
            return Container.Resolve<AppShell>(new NamedParameterOverloads() { { "isStaff", isStaff } });
        }

        public static TinyIoCContainer Container => TinyIoCContainer.Current;

        public static void Initialize()
        {
            var currentAssembly = Assembly.GetExecutingAssembly();

            foreach (var type in currentAssembly.DefinedTypes
                .Where(e =>
                e.IsSubclassOf(typeof(Page)) ||
                e.IsSubclassOf(typeof(BaseViewModel))))
            {
                Container.Register(type.AsType());
            }

            Container.Register<ILeaderService, LeaderService>();
            Container.Register<IUserService, UserService>();
            Container.Register<IDevService, DevService>();
            Container.Register<IScannerService, ScannerService>();
            Container.Register<IRewardService, RewardService>();
            Container.Register<IBrowser, AuthBrowser>();
            Container.Register<IPushNotificationActionService, PushNotificationActionService>();
            Container.Register<INotificationRegistrationService, NotificationRegistrationService>();
        }

        public static void InitializeNativeInstallation(IDeviceInstallationService deviceInstallationService)
        {
            var currentAssembly = Assembly.GetExecutingAssembly();

            foreach (var type in currentAssembly.DefinedTypes
                .Where(e =>
                e.IsSubclassOf(typeof(Page)) ||
                e.IsSubclassOf(typeof(BaseViewModel))))
            {
                Container.Register(type.AsType());
            }

            Container.Register(deviceInstallationService);
        }
    }
}