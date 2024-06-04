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
    /// ViewModel for editing a user.
    /// </summary>
    public class EditUserViewModel
    {
        [Required]
        [Display(Name = "Username")]
        public string UserName { get; set; }

        [Required]
        public string Forename { get; set; }

        [Required]
        public string Surname { get; set; }

        [Required]
        public string Email { get; set; }

        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        [Required]
        public string Street { get; set; }

        [Required]
        public string City { get; set; }

        [Required]
        [Display(Name = "Post Code")]
        public string Postcode { get; set; }

        [Display(Name = "Status")]
        public EmployementStatus EmployementStatus { get; set; }

        [Display(Name = "Status")]
        public CustomerType CustomerType { get; set; }

        [Display(Name = "Joined")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime RegisteredAt { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; }

        [Display(Name = "Suspended")]
        public bool IsSuspended { get; set; }

        public string OldRole { get; set; }

        public ICollection<SelectListItem> Roles { get; set; }

        [Display(Name = "Role")]
        public string Role { get; set; }

        [Required]
        public decimal Salary { get; set; }

        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 1)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }
    }

    #pragma warning restore CS1591 // Restore warning for missing XML comments
}