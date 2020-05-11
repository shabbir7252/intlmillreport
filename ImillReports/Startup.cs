using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(ImillReports.Startup))]
namespace ImillReports
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
