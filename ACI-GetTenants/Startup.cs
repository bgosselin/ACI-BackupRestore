using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(ACI_GetTenants.Startup))]
namespace ACI_GetTenants
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
