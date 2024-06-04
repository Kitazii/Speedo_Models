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
    /// Represents shipping information.
    /// </summary>
    public class ShippingInfo
    {
        //Declare Properties for a SHIPPING INFO class
        [Key, ForeignKey("Order")] //setting foreign key to OrderId in Order class
        public int ShippingId { get; set; }
        public decimal ShippingCost { get; set; }
        public string ShippingType { get; set; }

        public bool ShippingToHomeAddress { get; set; }

        [Display(Name = "Street")]
        [Required]
        public string ShippingStreet { get; set; }

        [Display(Name = "City")]
        [Required]
        public string ShippingCity { get; set; }

        [Display(Name = "Post Code")]
        [Required]
        public string ShippingPostcode { get; set; }

        //Navigational Properties - SHIPPING INFO & ORDER
        public Order Order { get; set; }//ONE
    }

    #pragma warning restore CS1591 // Restore warning for missing XML comments
}