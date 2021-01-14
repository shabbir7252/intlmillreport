using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(ImillPda.Startup))]
namespace ImillPda
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
