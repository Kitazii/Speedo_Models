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
    /// Represents a payment.
    /// </summary>
    public class Payment
    {
        //Declaring attributes for Payment class

        [Key, ForeignKey("Order")]//setting foreign key to OrderId in Order class
        public int PaymentId { get; set; }

        [Display(Name = "Total Amount")]
        public decimal TotalAmount { get; set; }

        public string Details { get; set; }

        public bool Paid { get; set; }

        public bool IsRefunded { get; set; }

        //Navigational Properties - PAYMENT & ORDER
        public Order Order { get; set; }//ONE
    }

    #pragma warning restore CS1591 // Restore warning for missing XML comments
}