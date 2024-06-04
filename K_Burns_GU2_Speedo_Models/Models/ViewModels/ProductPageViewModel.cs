using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace K_Burns_GU2_Speedo_Models.Models.ViewModels
{
    #pragma warning disable CS1591 // Disable warning for missing XML comments

    /// <summary>
    /// ViewModel for displaying product details on a product page.
    /// </summary>
    public class ProductPageViewModel
    {
        [Key]
        public int ProductId { get; set; }

        [Required]
        [Display(Name = "Product Name")]
        public string ProductName { get; set; }

        [Required]
        [Display(Name = "Product Description")]
        public string ProductDescription { get; set; }

        [Required]
        public decimal Price { get; set; }

        public decimal DiscountPrice
        {
            get
            {
                decimal halfPrice = Price / 2;
                return Math.Floor(halfPrice * 100) / 100;
            }
        }


        [Display(Name = "Image")]
        public string ImageUrl { get; set; }//storing path to product image in the database

        [Display(Name = "Sale")]
        public bool OnSale { get; set; }

        [Required]
        public int Quantity { get; set; } //only accepts positive number

        public int StockLevel { get; set; }

        public List<Product> Products { get; set; }
    }

    #pragma warning restore CS1591 // Restore warning for missing XML comments
}