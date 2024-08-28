using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace K_Burns_GU2_Speedo_Models.Controllers
{
    /// <summary>
    /// Handles the home, about, and contact pages of the application.
    /// </summary>

    public class HomeController : Controller
    {
        /// <summary>
        /// Displays the home page.
        /// </summary>
        /// <returns>The home page view.</returns>
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Displays the about page.
        /// </summary>
        /// <returns>The about page view.</returns>
        public ActionResult About()
        {
            // Set the description message for the about page
            ViewBag.Message = "Speedo Models application description page.";

            return View();
        }

        /// <summary>
        /// Displays the contact page.
        /// </summary>
        /// <returns>The contact page view.</returns>
        public ActionResult Contact()
        {
            // Set the contact message for the contact page
            ViewBag.Message = "Speedo Models contact page.";

            return View();
        }

        /// <summary>
        /// Displays Tutorial Splash Screen.
        /// </summary>
        /// <returns>The Splash Screen page view.</returns>
        public ActionResult SplashScreen()
        {
            return View();
        }
    }
}