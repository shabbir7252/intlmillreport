using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Optimization;

namespace ItemInventory
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            Syncfusion.Licensing.SyncfusionLicenseProvider
                .RegisterLicense("MzI2MjI1QDMxMzgyZTMyMmUzMFBnRUJhMHl2ajdpdm9WQ2ZWRFRTKzVlajhjUE1pdXFnaS9teHdxWXZiYjQ9");
            
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            BundleTable.EnableOptimizations = true;
        }
    }
}
