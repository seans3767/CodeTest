using System.ComponentModel.DataAnnotations;

namespace ConsoleApp1.Models
{
    public class PriceRequest
    {
        [Required(ErrorMessage  = "Risk Data is missing")]
        public RiskData RiskData { get; set; }
    }
}
