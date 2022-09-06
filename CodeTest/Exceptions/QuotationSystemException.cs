using System;

namespace ConsoleApp1.Exceptions
{
    public class QuotationSystemException : Exception
    {
        public QuotationSystemException() { }

        public QuotationSystemException(string message)
            : base(message)
        {

        }
    }
}
