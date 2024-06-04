using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace K_Burns_GU2_Speedo_Models.Models
{
    #pragma warning disable CS1591 // Disable warning for missing XML comments

    /// <summary>
    /// Represents a customer user.
    /// Which is a child class of 'User'
    /// </summary>
    public class Customer : User
    {
        //Declare Properties for a CUSTOMER user
        [Display(Name = "Status")]
        public CustomerType CustomerType { get; set; }

    }

    /// <summary>
    /// Enum for customer types.
    /// .
    /// 
    /// Premium(Indvidual) = faster, cheaper shipping
    ///Corporate(Business) = faster, cheaper shipping
    ///Standard(Indvidual) = default
    /// </summary>
    public enum CustomerType
    { 
        Premium,
        Corporate,
        Standard
    }

    #pragma warning restore CS1591 // Restore warning for missing XML comments
}