using System;
using System.ComponentModel.DataAnnotations;

namespace ConsoleApp1.Models
{
    public class RiskData
    {
        [Required(ErrorMessage = "First name is required")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Surname is required")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Value is required")]
        public decimal? Value { get; set; }

        public string Make { get; set; }

        public DateTime? DOB { get; set; }
    }
}
