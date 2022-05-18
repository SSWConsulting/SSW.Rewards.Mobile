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

            var resolvedType = TinyIoCContainer.Current.Resolve(typeToResolve);

            return (T)resolvedType;
        }

        public static AppShell ResolveShell(bool isStaff)
        {
            var container = TinyIoCContainer.Current;
            return container.Resolve<AppShell>(new NamedParameterOverloads() { { "isStaff", isStaff } });
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
            Container.Register<IChallengeService, ChallengeService>();
        }
    }
}
