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
    /// Represents an order.
    /// </summary>
    public class Order
    {
        //Declare Properties for a ORDER class
        [Key]
        public int OrderId { get; set; }

        public int OrderSize { get; set; }

        [Display(Name = "Order Date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:d}")] //Format as ShortDateTime
        public DateTime OrderDate { get; set; }

        [Display(Name = "Delivery Date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:d}")] //Format as ShortDateTime
        public DateTime DeliveryDate { get; set; }

        [Display(Name = "Order Total")]
        public decimal OrderTotal { get; set; }

        [Display(Name = "Final Total Cost")]
        public decimal TotalAmount { get; set; }

        public Status Status { get; set; }

        //Navigational Properties - ORDER & USER
        [ForeignKey("User")]
        public string Id { get; set; }
        public User User { get; set; }//ONE


        //Navigational Properties - ORDER & PAYMENT
        public Payment Payment { get; set; }//ONE

        //Navigational Properties - ORDER & SHIPPING INFO
        public ShippingInfo ShippingInfo { get; set; }//ONE

        //Navigational Properties - ORDER & PARCEL
        public Parcel Parcel { get; set; }

        //Navigational Properties - ORDER & ORDERLINE
        public List<OrderLine> OrderLines { get; set; }//MANY

        //Navigational Properties - ORDER & INVOICE
        public Invoice Invoice { get; set; }//ONE
    }

    /// <summary>
    /// Enumeration for order status.
    /// </summary>
    public enum Status
    {
        Pending,
        Packaging,
        Dispatched,
        Delivered,
    }

    #pragma warning restore CS1591 // Restore warning for missing XML comments
}