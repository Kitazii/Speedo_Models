using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace K_Burns_GU2_Speedo_Models.Models
{
    //public class Card
    //{
    //    //Declaring attributes for Card class

    //    [Key]
    //    public int CardId { get; set; }

    //    [Display(Name = "Name on the card")]
    //    public string CardholderName { get; set; }

    //    [Display(Name = "Card Number")]
    //    public string CardNumber { get; set; }

    //    [Display(Name = "CVV Code")]
    //    public string CvvCode { get; set; }

    //    [Display(Name = "MM / YY")]
    //    public string ExpiryDate { get; set; }

    //    public CardType CardType { get; set; }

    //    //Navigational Properties - CARD & CUSTOMER

    //    [ForeignKey("Customer")]
    //    public string UserId { get; set; }
    //    public Customer Customer { get; set; }//ONE

    //    //Navigational Properties - CARD & Payments
    //    public virtual ICollection<Payment> Payments { get; set; }//MANY

    //    //0 args CONSTRUCTOR
    //    public Card()
    //    {
    //        Payments = new List<Payment>();
    //    }
    //}

    //public enum CardType
    //{
    //    Mastercard,
    //    Visa,
    //    AmericanExpress
    //}
}