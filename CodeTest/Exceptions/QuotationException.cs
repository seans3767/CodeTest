using System;

namespace ConsoleApp1.Exceptions
{
    public class QuotationException : Exception
    {
        public QuotationException() { }

        public QuotationException(string message)
            : base(message)
        {

        }
    }
}
