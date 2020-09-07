
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Http;
using System.Web.Routing;

namespace ImillReports
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            // Old License Key = MjIyMzYzQDMxMzcyZTM0MmUzMEozVk1KeVAwN1owUndhQVZLdlBHRDZqZ24rem0zcHdYbHc4TllNYVg3Tk09
            // Old Licence Key V.52 = MjcwMjA1QDMxMzgyZTMxMmUzMERwaVMyUklkbXkrbkYwRHhjT0p5MWlUQlpFelcxZWdCOFI5RlgwaCtMdnc9
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("MjcwNDAwQDMxMzgyZTMxMmUzMFR3bW5vNWpEeVB4MVhoblgrM01iRWhiRlpwQ2tqOU5BWXRWNTBKQWNGVzQ9");

            // UnityConfig.RegisterComponents();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            BundleTable.EnableOptimizations = true;
        }
    }
}
