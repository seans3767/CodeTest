using System.Collections.Generic;

namespace ConsoleApp1.QuotationSystems
{
    internal class QuotationSystemProvider : IQuotationSystemProvider
    {
        public IEnumerable<IQuotationSystem> GetAll()
        {
            return new List<IQuotationSystem>()
            {
                new QuotationSystem1(),
                new QuotationSystem2(),
                new QuotationSystem3()
            };
        }
    }
}
