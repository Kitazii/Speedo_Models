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
    /// Represents a shopping basket.
    /// </summary>
    public class Basket
    {
        //Declare Properties for a BASKET class
        [Key]
        public int BasketId { get; set; }

        public int BasketSize { get; set; }

        public bool BasketAbandoned { get; set; }

        public decimal BasketTotal { get; set; }

        public decimal TotalAmount { get; set; }

        //Navigational Properties - BASKET & CUSTOMER
        [ForeignKey("User")]
        public string Id { get; set; }
        public User User { get; set; }//ONE

        //Navigational Properties - BASKET & BASKET ITEMS
        public List<BasketItem> BasketItems { get; set; }
    }

    #pragma warning restore CS1591 // Restore warning for missing XML comments
}