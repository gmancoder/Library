using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute("WebStartup", typeof(BookLibrary.Web.WebStartup))]
namespace BookLibrary.Web
{
    public partial class WebStartup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
