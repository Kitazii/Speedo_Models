using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace K_Burns_GU2_Speedo_Models.Models
{
    #pragma warning disable CS1591 // Disable warning for missing XML comments

    /// <summary>
    /// Represents the Speedo Models database context.
    /// </summary>
    public class SpeedoModelsDbContext : IdentityDbContext<User>
    {
        /// <summary>
        /// Declare database sets (Similiar to List Collection) attributes
        /// </summary>
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Basket> Baskets { get; set; }
        public DbSet<BasketItem> BasketItems { get; set; }
        public DbSet<OrderLine> OrderLines { get; set; }
        public DbSet<ShippingInfo> ShippingInfos { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<Parcel> Parcels { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SpeedoModelsDbContext"/> class.
        /// </summary>
        public SpeedoModelsDbContext()
            : base("GU2_Speedo_Models", throwIfV1Schema: false)
        {
            Database.SetInitializer(new DatabaseInitialiser());
        }

        /// <summary>
        /// Creates a new instance of the <see cref="SpeedoModelsDbContext"/> class.
        /// </summary>
        /// <returns>A new instance of the <see cref="SpeedoModelsDbContext"/> class.</returns>
        public static SpeedoModelsDbContext Create()
        {
            return new SpeedoModelsDbContext();
        }
    }


    #pragma warning restore CS1591 // Restore warning for missing XML comments
}