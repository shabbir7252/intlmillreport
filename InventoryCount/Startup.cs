using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(InventoryCount.Startup))]
namespace InventoryCount
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
