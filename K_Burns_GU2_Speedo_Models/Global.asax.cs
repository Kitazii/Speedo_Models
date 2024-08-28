using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace K_Burns_GU2_Speedo_Models
{
    /// <summary>
    /// Represents the main application class.
    /// </summary>
    public class MvcApplication : System.Web.HttpApplication
    {
        /// <summary>
        /// Handles the application start event.
        /// </summary>
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            var secretKey = WebConfigurationManager.AppSettings["StripeSecretKey"];
            StripeConfiguration.SetApiKey(secretKey);
        }
    }
}
