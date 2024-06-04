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
    /// Represents a parcel.
    /// </summary>
    public class Parcel
    {
        //Declare Properties for a SPARCEL class
        [Key, ForeignKey("Order")] //setting foreign key to OrderId in Order class
        public int ParcelId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Price must be greater than 0")]
        [Display(Name = "Weight (g)")]
        public int Weight { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Price must be greater than 0")]
        [Display(Name = "Width (cm)")]
        public int Width { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Price must be greater than 0")]
        [Display(Name = "Height (cm)")]
        public int Height { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Price must be greater than 0")]
        [Display(Name = "Length (cm)")]
        public int Length { get; set; }

        [Display(Name = "Parcel Type")]
        public ParcelType ParcelType { get; set; }
        public Method Method { get; set; }

        //Navigational Properties - PARCEL & ORDER
        public Order Order { get; set; }//ONE
    }

    /// <summary>
    /// Enumeration for parcel type.
    /// </summary>
    public enum ParcelType { Small,Medium,Big }

    /// <summary>
    /// Enumeration for delivery method.
    /// </summary>
    public enum Method { ParcelForce,RoyalMail}

    #pragma warning restore CS1591 // Restore warning for missing XML comments
}