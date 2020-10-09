using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Racoonogram.Startup))]
namespace Racoonogram
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
