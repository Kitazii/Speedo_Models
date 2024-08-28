using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace K_Burns_GU2_Speedo_Models.Models
{
    #pragma warning disable CS1591 // Disable warning for missing XML comments

    /// <summary>
    /// Represents an item in a shopping basket.
    /// </summary>
    public class BasketItem
    {
        [Key]
        public int BasketItemId { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Display(Name = "Item Total")]
        public decimal ItemTotal { get; set; }

        //Navigational Properties - BASKETITEM & PRODUCT

        [ForeignKey("Product")]
        public int ProductId { get; set; }
        public Product Product { get; set; }//ONE

        //Navigational Properties - BASKETITEM & BASKET

        [ForeignKey("Basket")]
        public int BasketId { get; set; }
        public Basket Basket { get; set; }//ONE
    }

    #pragma warning restore CS1591 // Restore warning for missing XML comments
}