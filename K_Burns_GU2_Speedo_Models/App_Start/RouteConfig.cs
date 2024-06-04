using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace K_Burns_GU2_Speedo_Models
{
    /// <summary>
    /// Configures the route mappings for the application.
    /// </summary>
    public class RouteConfig
    {
        /// <summary>
        /// Registers the routes for the application.
        /// </summary>
        /// <param name="routes">The route collection to which the routes will be added.</param>
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
