using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace K_Burns_GU2_Speedo_Models.Models.ViewModels
{
    #pragma warning disable CS1591 // Disable warning for missing XML comments

    /// <summary>
    /// ViewModel for handling basket and shipping information.
    /// </summary>
    public class BasketShippingViewModel
    {
        public Basket Basket { get; set; }
        public ShippingInfo ShippingInfo { get; set; }
        public bool PageReloaded { get; set; }
    }

    #pragma warning restore CS1591 // Restore warning for missing XML comments
}