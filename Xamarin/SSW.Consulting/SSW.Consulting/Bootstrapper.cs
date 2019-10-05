using Autofac;
using System.Linq;
using Xamarin.Forms;
using SSW.Consulting.Views;
using SSW.Consulting.ViewModels;
using SSW.Consulting.Services;
using System.Reflection;

namespace SSW.Consulting
{
    public abstract class Bootstrapper
    {
        protected ContainerBuilder ContainerBuilder { get; private set; }

        public Bootstrapper()
        {
            Initialize();
            FinishedInitialization();
        }

        protected virtual void Initialize()
        {
            var currentAssembly = Assembly.GetExecutingAssembly();
            ContainerBuilder = new ContainerBuilder();

            foreach(var type in currentAssembly.DefinedTypes
                .Where(e =>
                e.IsSubclassOf(typeof(Page)) ||
                e.IsSubclassOf(typeof(BaseViewModel))))
            {
                ContainerBuilder.RegisterType(type.AsType());
            }

            //If adding a repository or DB, add here as a singleton, e.g.:
            //ContainerBuilder.RegisterType<LeadersRepository>().SingleInstance();
            ContainerBuilder.RegisterType<LeaderService>().As<ILeaderService>();
            ContainerBuilder.RegisterType<UserService>().As<IUserService>();
            ContainerBuilder.RegisterType<MockDevService>().As<IDevService>();
            ContainerBuilder.RegisterType<MockChallengeService>().As<IChallengeService>();
        }

        protected void FinishedInitialization()
        {
            var container = ContainerBuilder.Build();
            Resolver.Initialize(container);
        }
    }
}
