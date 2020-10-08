using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(ItemInventory.Startup))]
namespace ItemInventory
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
