using IntlmillReports.Contracts;
using IntlmillReports.Repository;
using System.Web.Mvc;
using Unity;
using Unity.Mvc5;

namespace IntlmillReports
{
    public static class UnityConfig
    {
        public static void RegisterComponents()
        {
			var container = new UnityContainer();

            // register all your components with the container here
            // it is NOT necessary to register your controllers

            // e.g. container.RegisterType<ITestService, TestService>();

            container.RegisterType<ISalesReportRepository, SalesReportRepository>();
            container.RegisterType<ILocationRepository, LocationRepository>();
            container.RegisterType<ISalesmanRepository, SalesmanRepository>();
            DependencyResolver.SetResolver(new UnityDependencyResolver(container));
        }
    }
}