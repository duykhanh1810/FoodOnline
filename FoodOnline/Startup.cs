using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(FoodOnline.Startup))]
namespace FoodOnline
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            //ConfigureAuth(app);
            app.MapSignalR();
        }
    }
}
