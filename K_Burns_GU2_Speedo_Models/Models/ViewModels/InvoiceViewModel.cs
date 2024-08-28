using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace K_Burns_GU2_Speedo_Models.Models.ViewModels
{
    #pragma warning disable CS1591 // Disable warning for missing XML comments

    /// <summary>
    /// ViewModel for displaying an invoice with related order and user information.
    /// </summary>
    public class InvoiceViewModel
    {
        public Invoice Invoice { get; set; }
        public Order Order { get; set; }
        public User User { get; set; }

        [Display(Name = "Total Products")]
        public List<OrderLine> OrderLines { get; set; }

        [Display(Name = "Delivery Charges")]
        public ShippingInfo Shipping { get; set; }

        public ICollection<SelectListItem> OrderLineItems { get; set; }
    }

    #pragma warning restore CS1591 // Restore warning for missing XML comments
}