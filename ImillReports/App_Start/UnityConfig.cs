using Unity;
using System.Web.Mvc;
using Unity.Mvc5;
using Unity.Injection;
using ImillReports.Models;
using ImillReports.Contracts;
using ImillReports.Repository;
using ImillReports.Controllers;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace ImillReports
{
    public static class UnityConfig
    {
        public static void RegisterComponents()
        {
			var container = new UnityContainer();

            // register all your components with the container here
            // it is NOT necessary to register your controllers

            // e.g. container.RegisterType<ITestService, TestService>();

            container.RegisterType<IUserStore<ApplicationUser>, UserStore<ApplicationUser>>();
            container.RegisterType<UserManager<ApplicationUser>>();
            // container.RegisterType<DbContext, ApplicationDbContext>();
            container.RegisterType<ApplicationUserManager>();
            container.RegisterType<AccountController>(new InjectionConstructor());

            container.RegisterType<ISalesReportRepository, SalesReportRepository>();
            container.RegisterType<ILocationRepository, LocationRepository>();
            container.RegisterType<ISalesmanRepository, SalesmanRepository>();
            container.RegisterType<IDashboardRepository, DashboardRepository>();
            container.RegisterType<ICashRegisterRepository, CashRegisterRepository>();
            container.RegisterType<IBaseUnitRepository, BaseUnitRepository>();
            container.RegisterType<IVoucherTypesRepository, VoucherTypesRepository>();
            container.RegisterType<IProductRepository, ProductRepository>();
            container.RegisterType<IBaseRepository, BaseRepository>();
            DependencyResolver.SetResolver(new UnityDependencyResolver(container));
        }
    }
}