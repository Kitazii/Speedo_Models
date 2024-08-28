using System.Web;
using System.Web.Mvc;

namespace K_Burns_GU2_Speedo_Models
{
    /// <summary>
    /// Configures the global filters for the application.
    /// </summary>
    public class FilterConfig
    {
        /// <summary>
        /// Registers the global filters for the application.
        /// </summary>
        /// <param name="filters">The global filter collection to which filters will be added.</param>
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
