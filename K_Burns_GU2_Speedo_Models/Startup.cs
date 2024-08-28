using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(K_Burns_GU2_Speedo_Models.Startup))]
namespace K_Burns_GU2_Speedo_Models
{
    public partial class Startup
    {
        /// <summary>
        /// Configures the application.
        /// </summary>
        /// <param name="app">The application builder.</param>
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
