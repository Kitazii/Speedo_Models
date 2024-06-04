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
    /// Represents an order line.
    /// </summary>
    public class OrderLine
    {
        //Declare Properties for a ORDER-LINE class
        [Key]
        public int OrderLineId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Ensure That The quantity Is Greater Than 0")]
        public int Quantity { get; set; }

        [Display(Name = "Line Total")]
        public decimal LineTotal { get; set; }

        //Navigational Properties - ORDERLINE & PRODUCT
        [ForeignKey("Product")]
        public int ProductId { get; set; }
        public Product Product { get; set; }//ONE

        //Navigational Properties - ORDERLINE & ORDER
        [ForeignKey("Order")]
        public int OrderId { get; set; }
        public Order Order { get; set; }//MANY
    }

    #pragma warning restore CS1591 // Restore warning for missing XML comments
}