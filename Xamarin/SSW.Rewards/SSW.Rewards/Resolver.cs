using IdentityModel.OidcClient.Browser;
using SSW.Rewards.Helpers;
using SSW.Rewards.Services;
using SSW.Rewards.ViewModels;
using System;
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

            Console.WriteLine($"Resolving {typeToResolve.Name}");

            var resolvedType = Container.Resolve(typeToResolve);

            if (resolvedType == null)
            {
                Console.WriteLine("FAILED resolving. Resolved type is null");
            }
            else
            {
                Console.WriteLine($"Resolved {resolvedType.GetType().Name}");
            }

            return (T)resolvedType;
        }

        public static AppShell ResolveShell(bool isStaff)
        {
            return Container.Resolve<AppShell>(new NamedParameterOverloads() { { "isStaff", isStaff } });
        }

        public static TinyIoCContainer Container => TinyIoCContainer.Current;

        public static void Initialize()
        {
            try
            {
                Console.WriteLine("Attempting to register ViewModels and Pages");

                var currentAssembly = Assembly.GetExecutingAssembly();

                foreach (var type in currentAssembly.DefinedTypes
                    .Where(e =>
                    e.IsSubclassOf(typeof(Page)) ||
                    e.IsSubclassOf(typeof(BaseViewModel))))
                {
                    Console.WriteLine($"Registering {type.Name}");
                    Container.Register(type.AsType());
                }

                Container.Register<ILeaderService, LeaderService>();
                Container.Register<IUserService, UserService>();
                Container.Register<IDevService, DevService>();
                Container.Register<IChallengeService, ChallengeService>();
                Container.Register<IBrowser, AuthBrowser>();

                Console.WriteLine("Resolver initialisation completed successfully.");
            }
            catch (System.Exception ex)
            {
                Console.WriteLine("ERROR: Resolver initialisation failed.");
                Console.WriteLine(ex.Message);
            }
        }
    }
}