using System.Reflection;
using System.Web.Http;
using System.Web.Mvc;
using Autofac;
using Autofac.Integration.Mvc;
using Autofac.Integration.WebApi;
using Geonorge.Kartografi.Models;

namespace Geonorge.Kartografi.App_Start
{
    public class DependencyConfig
    {
        public static void Configure(ContainerBuilder builder)
        {
            ConfigureInfrastructure(builder);

            ConfigureApplicationDependencies(builder);

            var container = builder.Build();

            SetupAspMvcDependencyResolver(container);
        }

        private static void ConfigureApplicationDependencies(ContainerBuilder builder)
        {
            builder.RegisterType<CartographyDbContext>().InstancePerRequest().AsSelf();
            
        }

        private static void SetupAspMvcDependencyResolver(IContainer container)
        {
            // dependency resolver for MVC
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
        }

        private static void ConfigureInfrastructure(ContainerBuilder builder)
        {
            builder.RegisterControllers(Assembly.GetExecutingAssembly());
            builder.RegisterModule(new AutofacWebTypesModule());
        }
    }
}