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
    /// Represents a product.
    /// </summary>
    public class Product
    {
        //Declare Properties for a PRODUCT class
        [Key]
        public int ProductId { get; set; }

        [Required]
        [Display(Name = "Product Name")]
        public string ProductName { get; set; }

        [Required]
        [Display(Name = "Product Description")]
        public string ProductDescription { get; set; }

        public bool IsDeleted { get; set; }

        [Required]
        public decimal Price { get; set; }

        private decimal? _discountPrice;

        [Display(Name = "Product Discount")]
        public decimal DiscountPrice 
        {
            get 
            {
                // If _discountPrice has been explicitly set, return it
                if (_discountPrice.HasValue)
                {
                    return _discountPrice.Value;
                }

                decimal halfPrice = Price / 2;
                return Math.Floor(halfPrice * 100) / 100;
            }
            set
            {
                _discountPrice = value;
            }
        }

        [Required]
        [Display(Name = "Stock Level")]
        public int StockLevel { get; set; }

        [Display(Name = "Image")]
        public string ImageUrl { get; set; }//storing path to product image in the database

        [Display(Name = "Sale")]
        public bool OnSale { get; set; }

        public bool Discontinued { get; set; }

        [Display(Name = "Date Created")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:d}")] //Format as ShortDateTime
        public DateTime DateCreated { get; set; }

        [Display(Name = "Product Size (cm)")]
        public int ProductSize { get; set; }

        //Navigational Properties - PRODUCT & CATEGORY
        [ForeignKey("Category")]
        public int CategoryId { get; set; }
        public Category Category { get; set; }//ONE

        //Navigational Properties - PRODUCT & ORDERLINE
        public List<OrderLine> OrderLines { get; set; }//MANY

        //Navigational Properties - PRODUCT & BASKETITEMS
        public List<BasketItem> BasketItems { get; set; }//MANY

        // Method to truncate a string to a maximum number of words
        public string TruncateDescription(int wordLimit)
        {
            //ensures to return if empty or has empty spacing
            if (string.IsNullOrWhiteSpace(ProductDescription))
            {
                return ProductDescription;
            }

            //splits the input string into an array of words using a space as the delimiter.
            //if there are any empty back to back spaces we remove it before storing the word
            var words = ProductDescription.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            //if the words length is less than the wordLimit then we return the string as it is
            if (words.Length <= wordLimit)
            {
                return ProductDescription;
            }

            //otherwise we join the array, truncate the words and add a "..." to the end of the string
            return string.Join(" ", words.Take(wordLimit)) + "...";
        }
    }

    #pragma warning restore CS1591 // Restore warning for missing XML comments
}