using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace K_Burns_GU2_Speedo_Models.Models.ViewModels
{
    #pragma warning disable CS1591 // Disable warning for missing XML comments

    /// <summary>
    /// ViewModel for the advanced dashboard, providing detailed information about users and their orders.
    /// </summary>
    public class AdvancedDashboardViewModels
    {
        public User User { get; set; }
        public List<Order> Orders { get; set; }
        public int OrdersCount { get; set; }
        public int ProductsCount { get; set; }
        public int UsersCount { get; set; }
        public int InvoicesCount { get; set; }
        public decimal TotalAmounts { get; set; }
    }

    #pragma warning restore CS1591 // Restore warning for missing XML comments
}