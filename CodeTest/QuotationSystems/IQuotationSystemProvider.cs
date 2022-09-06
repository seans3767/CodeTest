using System.Collections.Generic;

namespace ConsoleApp1.QuotationSystems
{
    public interface IQuotationSystemProvider
    {
        IEnumerable<IQuotationSystem> GetAll();
    }
}
