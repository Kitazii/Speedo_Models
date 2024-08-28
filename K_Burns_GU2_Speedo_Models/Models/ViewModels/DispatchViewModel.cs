using Stripe;
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
    /// ViewModel for managing dispatches.
    /// </summary>
    public class DispatchViewModel
    {
        public Order Order { get; set; }
        public User User { get; set; }
        public ShippingInfo Shipping { get; set; }
        public Parcel Parcel { get; set; }

        [Display(Name = "Items")]
        public List<OrderLine> OrderLines { get; set; }
        public ICollection<SelectListItem> OrderLineItems { get; set; }

        [Display(Name = "Delivery Name")]
        public string DeliveryName
        {
            get
            {
                return $"{User.Forename} {User.Surname}";
            }
        }

        // Define the DeliveryAddress property to return formatted address
        [Display(Name = "Delivery Address")]
        public string DeliveryAddress
        {
            get
            {
                return $"{Shipping.ShippingStreet}, {Shipping.ShippingCity}, {Shipping.ShippingPostcode}";
            }
        }

        public string Items
        {
            get
            {
                string text = "";
                int count = 0;
                int quantityCount = 0;

                foreach(var item in OrderLines)
                {
                    quantityCount = item.Quantity;

                    while (quantityCount > 0)
                    {
                        if (count % 3 == 2) //checks every third item
                        {
                            text += "\n"; //to send the text to a new line
                        }
                        //this gets the last item in the orderline
                        if (OrderLines[OrderLines.Count-1] == item && quantityCount == 1)
                        {
                            //so that we dont display ',' at the end
                            text += item.Product.ProductName;
                            quantityCount--;
                        }
                        else
                        {
                            text += item.Product.ProductName + ",";
                            quantityCount--;
                        }
                        count++;
                    }
                        
                }
                return text;
            }
        }

    }
    #pragma warning restore CS1591 // Restore warning for missing XML comments
}