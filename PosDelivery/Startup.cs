using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(PosDelivery.Startup))]
namespace PosDelivery
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
