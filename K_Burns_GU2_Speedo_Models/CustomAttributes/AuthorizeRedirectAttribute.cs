using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace K_Burns_GU2_Speedo_Models.CustomAttributes
{
    /// <summary>
    /// Custom authorization attribute that redirects unauthorized users to a specified URL.
    /// </summary>
    public class AuthorizeRedirectAttribute : AuthorizeAttribute
    {
        /// <summary>
        /// Gets or sets the URL to redirect unauthorized users to. Default is "/".
        /// </summary>
        public string RedirectUrl { get; set; } = "/"; // Default redirect URL.

        /// <summary>
        /// Determines whether access to the core framework is authorized.
        /// </summary>
        /// <param name="httpContext">The HTTP context, which encapsulates all HTTP-specific information about an individual HTTP request.</param>
        /// <returns>True if the user is authorized; otherwise, false.</returns>
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            bool authorized = base.AuthorizeCore(httpContext);
            if (!authorized)
            {
                // The user is not authenticated.
                return false;
            }

            if (this.Roles.Split(',').Any(role => httpContext.User.IsInRole(role)))
            {
                // The user is authenticated and has one of the required roles.
                return true;
            }

            // The user is authenticated but does not have the required roles.
            httpContext.Response.Redirect(this.RedirectUrl);
            return false;
        }

        /// <summary>
        /// Handles unauthorized requests.
        /// </summary>
        /// <param name="filterContext">Encapsulates the information needed to use the <see cref="AuthorizeAttribute"/>.</param>
        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            // Check if the user is authenticated but not in the required role
            if (filterContext.HttpContext.Request.IsAuthenticated)
            {
                // Redirect to the specified URL
                filterContext.Result = new RedirectResult(RedirectUrl);
            }
            else
            {
                // Call the base method to handle usual unauthorized cases (like not being authenticated at all)
                base.HandleUnauthorizedRequest(filterContext);
            }
        }
    }
}