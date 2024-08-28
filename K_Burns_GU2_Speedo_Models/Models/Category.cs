using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace K_Burns_GU2_Speedo_Models.Models
{
    #pragma warning disable CS1591 // Disable warning for missing XML comments

    /// <summary>
    /// Represents a product category.
    /// </summary>
    public class Category
    {
        //Declare Properties for a CATEGORY class
        [Key]
        public int CategoryId { get; set; }

        [Display(Name = "Category Name")]
        public string CategoryName { get; set; }

        //Navigational Properties - CATEGORY & PRODUCT
        public virtual ICollection<Product> Products { get; set; }//MANY

        //0 args CONSTRUCTOR
        public Category()
        {
            Products = new List<Product>();
            
        }
    }

    #pragma warning restore CS1591 // Restore warning for missing XML comments
}