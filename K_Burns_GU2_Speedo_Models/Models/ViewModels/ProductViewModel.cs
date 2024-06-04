using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace K_Burns_GU2_Speedo_Models.Models.ViewModels
{
    #pragma warning disable CS1591 // Disable warning for missing XML comments

    /// <summary>
    /// ViewModel for creating and managing product details.
    /// </summary>
    public class ProductViewModel
    {
        [Required]
        public int ProductId { get; set; }
        [Required]
        [Display(Name = "Product Name")]
        public string ProductName { get; set; }

        [Required]
        [Display(Name = "Product Description")]
        public string ProductDescription { get; set; }

        [Required]
        [Range(0.1, double.MaxValue, ErrorMessage = "Price must be greater than 0.0.")]
        public decimal Price { get; set; }

        [Display(Name = "Sale Difference(%)")]
        [Range(0, 99, ErrorMessage = "Sales difference must be between 0 and 99.")]
        public int SaleDifference { get; set; }

        private decimal? _discountPrice;

        public decimal DiscountPrice
        {
            get
            {
                decimal difference = Price * ((decimal)SaleDifference / 100);
                decimal newPrice = Price - difference;
                return newPrice;
            }

            set
            {
                _discountPrice = value;
            }
        }

        [Required]
        [Display(Name = "Stock Level")]
        [Range(0, int.MaxValue, ErrorMessage = "Stock Level must be 0 or more.")]
        public int StockLevel { get; set; }

        [Display(Name = "Image")]
        public string ImageUrl { get; set; }//storing path to product image in the database

        [Display(Name = "Sale")]
        public bool OnSale { get; set; }

        public bool Discontinued { get; set; }

        [Display(Name = "Size(cm)")]
        [Range(1, int.MaxValue, ErrorMessage = "Product size must be greater than 0.")]
        public int ProductSize { get; set; }
        public string Category { get; set; }//ONE
        public ICollection<SelectListItem> Categories { get; set; }
    }

    #pragma warning restore CS1591 // Restore warning for missing XML comments
}