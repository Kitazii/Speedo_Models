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
    /// Represents an invoice.
    /// </summary>
    public class Invoice
    {
        //Declare Properties for a INVOICE class
        [Key, ForeignKey("Order")]
        public int InvoiceId { get; set; }

        [Display(Name = "Invoice Date")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = "{0:d}")] //Format as ShortDateTime
        public DateTime InvoiceDate { get; set; }

        [Display(Name = "Receipts Voucher")]
        public string ReceiptsVoucher { get; set; }

        //Navigational Properties - INVOICE & ORDER
        [ForeignKey("Order")]
        public Order Order { get; set; }//ONE

        // Additional property to return formatted date
        public string FormattedInvoiceDate => InvoiceDate.ToString("d MMMM yyyy");
    }

    #pragma warning restore CS1591 // Restore warning for missing XML comments
}