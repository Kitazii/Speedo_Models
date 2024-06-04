using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace K_Burns_GU2_Speedo_Models.Models
{
    #pragma warning disable CS1591 // Disable warning for missing XML comments

    /// <summary>
    /// Represents an employee.
    /// Which is a child class of 'User'
    /// </summary>
    public class Employee : User
    {
        //Declare Properties for a CUSTOMER user
        public bool Promote { get; set; }

        private decimal salary; //use a backing field for the salary
        [Required]
        public decimal Salary
        {
            get => salary;
            set
            {
                salary = value;
                //automatically update the SalaryOutput when salary is set
                SalaryOutput = FormatSalaryWithCommas(salary);
            }
        }

        [Display(Name = "Salary")]
        public string SalaryOutput { get; set; }

        [Display(Name = "Status")]
        public EmployementStatus EmployementStatus { get; set; }

        //increases readability
        //formats salary with commas
        private string FormatSalaryWithCommas(decimal salary)
        {
            return string.Format("£{0:#,0.##}", salary);
        }

    }

    /// <summary>
    /// Enumeration for employment status.
    /// </summary>
    public enum EmployementStatus
    {
        FullTime,
        PartTime
    }

    #pragma warning restore CS1591 // Restore warning for missing XML comments
}