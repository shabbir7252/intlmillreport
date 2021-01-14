using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace ImillPda
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            //Syncfusion.Licensing.SyncfusionLicenseProvider
            //    .RegisterLicense("MzI2MjI1QDMxMzgyZTMyMmUzMFBnRUJhMHl2ajdpdm9WQ2ZWRFRTKzVlajhjUE1pdXFnaS9teHdxWXZiYjQ9");

            Syncfusion.Licensing.SyncfusionLicenseProvider
                .RegisterLicense("MjcwNDAwQDMxMzgyZTMxMmUzMFR3bW5vNWpEeVB4MVhoblgrM01iRWhiRlpwQ2tqOU5BWXRWNTBKQWNGVzQ9");

            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            BundleTable.EnableOptimizations = true;
        }
    }
}
