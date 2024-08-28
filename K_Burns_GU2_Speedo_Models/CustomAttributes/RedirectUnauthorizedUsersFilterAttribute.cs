using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace K_Burns_GU2_Speedo_Models.Models
{
    /// <summary>
    /// Custom action filter attribute to redirect unauthorized users based on their roles.
    /// </summary>
    public class RedirectUnauthorizedUsersFilterAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// Called after an action method is executed.
        /// </summary>
        /// <param name="filterContext">The filter context.</param>
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            var user = filterContext.HttpContext.User;
            // Check if the requested path starts with a specific path
            var requestedPath = filterContext.HttpContext.Request.Path;

            //*******   Deal with all the logged in USER ROLES  **********

            // Check if the user is accessing the "/UserPage" route
            if (requestedPath.StartsWith("/UserPage", StringComparison.OrdinalIgnoreCase) &&
            user.Identity.IsAuthenticated && !(user.IsInRole("Admin")
            || user.IsInRole("Customer")
            || user.IsInRole("Sales Assistant")
            || user.IsInRole("Sales Manager")
            || user.IsInRole("Assistant Manager")
            || user.IsInRole("Stock Control Manager")
            || user.IsInRole("Invoice Clerk")
            || user.IsInRole("Warehouse Assistant")))
            {
                // Redirect authenticated users without the required role to the home page
                filterContext.Result = new RedirectResult("~/Home/Index");
            }

            // Check if the user is accessing the "/Admin" route
            if (requestedPath.StartsWith("/Admin", StringComparison.OrdinalIgnoreCase) &&
            user.Identity.IsAuthenticated && !user.IsInRole("Admin"))
            {
                // Redirect authenticated users without the required role to the home page
                filterContext.Result = new RedirectResult("~/Home/Index");
            }

            // Check if the user is accessing the "/SalesAssistant" route
            if (requestedPath.StartsWith("/SalesAssistant", StringComparison.OrdinalIgnoreCase) &&
            user.Identity.IsAuthenticated && !user.IsInRole("Sales Assistant"))
            {
                // Redirect authenticated users without the required role to the home page
                filterContext.Result = new RedirectResult("~/Home/Index");
            }

            // Check if the user is accessing the "/InvoiceClerk" route
            if (requestedPath.StartsWith("/InvoiceClerk", StringComparison.OrdinalIgnoreCase) &&
            user.Identity.IsAuthenticated && !user.IsInRole("Invoice Clerk"))
            {
                // Redirect authenticated users without the required role to the home page
                filterContext.Result = new RedirectResult("~/Home/Index");
            }

            // Check if the user is accessing the "/SalesManager" route
            if (requestedPath.StartsWith("/SalesManager", StringComparison.OrdinalIgnoreCase) &&
            user.Identity.IsAuthenticated && !user.IsInRole("Sales Manager"))
            {
                // Redirect authenticated users without the required role to the home page
                filterContext.Result = new RedirectResult("~/Home/Index");
            }

            // Check if the user is accessing the "/AssistantManager" route
            if (requestedPath.StartsWith("/AssistantManager", StringComparison.OrdinalIgnoreCase) &&
            user.Identity.IsAuthenticated && !user.IsInRole("Assistant Manager"))
            {
                // Redirect authenticated users without the required role to the home page
                filterContext.Result = new RedirectResult("~/Home/Index");
            }

            // Check if the user is accessing the "/StockControlManager" route
            if (requestedPath.StartsWith("/StockControlManager", StringComparison.OrdinalIgnoreCase) &&
            user.Identity.IsAuthenticated && !user.IsInRole("Stock Control Manager"))
            {
                // Redirect authenticated users without the required role to the home page
                filterContext.Result = new RedirectResult("~/Home/Index");
            }

            // Check if the user is accessing the "/WarehouseAssistant" route
            if (requestedPath.StartsWith("/WarehouseAssistant", StringComparison.OrdinalIgnoreCase) &&
            user.Identity.IsAuthenticated && !user.IsInRole("Warehouse Assistant"))
            {
                // Redirect authenticated users without the required role to the home page
                filterContext.Result = new RedirectResult("~/Home/Index");
            }

            // Check if the user is accessing the "/Customer" route
            if (requestedPath.StartsWith("/Customer", StringComparison.OrdinalIgnoreCase) &&
            user.Identity.IsAuthenticated && !user.IsInRole("Customer"))
            {
                // Redirect authenticated users without the required role to the home page
                filterContext.Result = new RedirectResult("~/Home/Index");
            }

            base.OnActionExecuted(filterContext);
        }
    }
}