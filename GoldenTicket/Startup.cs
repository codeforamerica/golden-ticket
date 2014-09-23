using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(GoldenTicket.Startup))]
namespace GoldenTicket
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
