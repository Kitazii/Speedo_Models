using System.Web;
using System.Web.Optimization;

namespace K_Burns_GU2_Speedo_Models
{
    /// <summary>
    /// Registers all the script and style bundles for the application.
    /// </summary>
    public class BundleConfig
    {
        /// <summary>
        /// Registers the script and style bundles.
        /// </summary>
        /// <param name="bundles">The bundle collection to which the bundles will be added.</param>
        /// <remarks>
        /// For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        /// </remarks>
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at https://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new Bundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/Site.css",
                      "~/Content/styles2.css",
                      "~/Content/styles.css"));
        }
    }
}
