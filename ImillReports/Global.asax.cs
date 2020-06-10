using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace ImillReports
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("MjIyMzYzQDMxMzcyZTM0MmUzMEozVk1KeVAwN1owUndhQVZLdlBHRDZqZ24rem0zcHdYbHc4TllNYVg3Tk09");

            // UnityConfig.RegisterComponents();
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            BundleTable.EnableOptimizations = true;
        }
    }
}
