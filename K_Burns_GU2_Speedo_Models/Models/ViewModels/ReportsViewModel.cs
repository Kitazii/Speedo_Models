using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace K_Burns_GU2_Speedo_Models.Models.ViewModels
{
    #pragma warning disable CS1591 // Disable warning for missing XML comments

    /// <summary>
    /// ViewModel for generating reports.
    /// </summary>
    public class ReportsViewModel
    {
        public List<Order> Orders { get; set; }
        public List<Invoice> Invoices { get; set; }
        public List <Product> Products { get; set; }
        public List<User> Users { get; set; }
        public string ReportType { get; set; }
    }

    #pragma warning restore CS1591 // Restore warning for missing XML comments
}