using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;

namespace K_Burns_GU2_Speedo_Models.Models
{
    #pragma warning disable CS1591 // Disable warning for missing XML comments
    /// <summary>
    /// Represents a user.
    /// 
    /// Parent Class of Employee and Customer
    /// </summary>

    public class User : IdentityUser
    {
        //Extend and declare properties for ALL users
        public string Forename { get; set; }

        public string Surname { get; set; }

        public string Street { get; set; }

        public string City { get; set; }

        [Display(Name = "Post Code")]
        public string Postcode { get; set; }

        [Display(Name = "Joined")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime RegisteredAt { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; }

        //Using the ApplicationUserManager to get the user's current role
        private ApplicationUserManager userManager;

        //Navigational Properties - USER & ORDER
        public List<Order> Orders { get; set; }//MANY

        //Navigational Properties - USER & BASKET
        public List<Basket> Basket { get; set; }//ONE

        //**************************************************************************************************************************

        //The CurrentRole property is not mapped as a field in the Users table
        //Its being used to get the current role that the user is logged in

        [Display(Name = "Role")]
        [NotMapped]
        public string CurrentRole
        {
            get
            {
                if (userManager == null)
                {
                    userManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
                }

                return userManager.GetRoles(Id).Single();
            }
        }
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<User> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }

    #pragma warning restore CS1591 // Restore warning for missing XML comments
}