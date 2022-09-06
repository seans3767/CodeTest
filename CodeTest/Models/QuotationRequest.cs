using System;

namespace ConsoleApp1.Models
{
    public class QuotationRequest
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public decimal Value { get; set; }
        public string Make { get; set; }
        public DateTime? DOB { get; set; }
    }
}
