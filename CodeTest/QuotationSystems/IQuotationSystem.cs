using ConsoleApp1.Models;
using System.Threading.Tasks;

namespace ConsoleApp1.QuotationSystems
{
    public interface IQuotationSystem
    {
        Task<QuotationResponse> GetPrice(QuotationRequest request);

        bool Accepts(QuotationRequest request);
    }
}
