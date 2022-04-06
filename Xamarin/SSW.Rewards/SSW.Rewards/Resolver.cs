using Autofac;

namespace SSW.Rewards
{
    public static class Resolver
    {
        private static IContainer container;

        public static void Initialize(IContainer container)
        {
            Resolver.container = container;
        }

        public static T Resolve<T>()
        {
            return container.Resolve<T>();
        }

        public static AppShell ResolveShell(bool isStaff)
        {
            return container.Resolve<AppShell>(new NamedParameter("isStaff", isStaff));
        }
    }
}
