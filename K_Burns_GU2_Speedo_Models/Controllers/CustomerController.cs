using K_Burns_GU2_Speedo_Models.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using K_Burns_GU2_Speedo_Models.Models.ViewModels;
using System.Net;

namespace K_Burns_GU2_Speedo_Models.Controllers
{
    /// <summary>
    /// only allow Customer to have authority to use this controller
    ///and redirect to main page if user is already logged in
    ///
    /// Handles customer-specific actions such as viewing invoices and returning orders.
    /// </summary>
    [RedirectUnauthorizedUsersFilter]
    public class CustomerController : AccountController
    {
        // Creating an instance of the Speedo Models DB Context
        private SpeedoModelsDbContext db = new SpeedoModelsDbContext();

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomerController"/> class.
        /// </summary>
        public CustomerController() : base() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomerController"/> class with specified user and sign-in managers.
        /// </summary>
        /// <param name="userManager">The user manager.</param>
        /// <param name="signInManager">The sign-in manager.</param>
        public CustomerController(ApplicationUserManager userManager, ApplicationSignInManager signInManager) : base(userManager, signInManager) { }

        /// <summary>
        /// Displays the list of invoices for the logged-in customer.
        /// </summary>
        /// <returns>The view containing the list of invoices.</returns>
        [HttpGet]
        public ActionResult MyInvoices()
        {
            // Get user by ID
            string userId = User.Identity.GetUserId();

            // Get the user's invoices from the database
            var myInvoices = db.Invoices
                .Include("Order.User")
                .Where(i => i.Order.User.Id == userId)
                .Include("Order.ShippingInfo")
                .ToList();

            // Return bad request if myinvoices object is null
            if (myInvoices == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Pass the myvoices object to the view
            return View(myInvoices);
        }

        /// <summary>
        /// Displays the details of a specific invoice.
        /// </summary>
        /// <param name="invoiceId">The ID of the invoice to view.</param>
        /// <returns>The view containing the invoice details.</returns>
        [HttpGet]
        public ActionResult InvoiceView(int? invoiceId)
        {
            // Get the specific invoice from the database
            Invoice invoice = db.Invoices
                .Where(i => i.InvoiceId == invoiceId)
                .Include("Order.ShippingInfo")
                .Include("Order.OrderLines.Product.Category")
                .Include("Order.User")
                .FirstOrDefault();

            // Return bad request if myinvoices object is null
            if (invoice == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Create a view model containing the invoice details
            InvoiceViewModel invoiceViewModel = new InvoiceViewModel
            { 
                Invoice = invoice,
                Order = invoice.Order,
                User = invoice.Order.User,
                OrderLines = invoice.Order.OrderLines,
                Shipping = invoice.Order.ShippingInfo
            };

            // Pass the invoiceViewModel object to the view
            return View(invoiceViewModel);
        }

        /// <summary>
        /// Displays the return order page.
        /// </summary>
        /// <returns>The return order view.</returns>
        [HttpGet]
        public ActionResult ReturnOrder()
        {
            return View();
        }
    }
}