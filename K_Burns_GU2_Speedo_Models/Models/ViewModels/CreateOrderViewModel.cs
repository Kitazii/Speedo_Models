using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace K_Burns_GU2_Speedo_Models.Models.ViewModels
{
    #pragma warning disable CS1591 // Disable warning for missing XML comments

    /// <summary>
    /// ViewModel for creating a new order.
    /// </summary>
    public class CreateOrderViewModel
    {
        public Order Order { get; set; }
        public User User { get; set; }
        public ShippingInfo Shipping { get; set; }
        public Product Product { get; set; }
        public List<OrderLine> OrderLines { get; set; }
        public int ProductAmount { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Ensure That The Product Count Is Greater Than 0")]
        [Display(Name = "Product Count")]
        public int ProductCount { get; set; }

        public int Quantity { get; set; }
    }
    #pragma warning restore CS1591 // Restore warning for missing XML comments
}