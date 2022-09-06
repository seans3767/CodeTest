using ConsoleApp1.Models;
using System;
using System.Dynamic;
using System.Threading.Tasks;

namespace ConsoleApp1.QuotationSystems
{
    internal class QuotationSystem3 : IQuotationSystem
    {
        private const string Url = "http://quote-system-3.com";
        private const string Port = "100";

        public bool Accepts(QuotationRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            return true;
        }

        public async Task<QuotationResponse> GetPrice(QuotationRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var system3Response = await SendRequest(request);

            if (!system3Response.IsSuccess)
            {
                throw new Exception("Request to Quotation System 1 failed");
            }

            var response = new QuotationResponse()
            {
                Price = system3Response.Price,
                Tax = system3Response.Tax,
                Insurer = system3Response.Name
            };

            return response;
        }

        private Task<System3Response> SendRequest(QuotationRequest request)
        {
            //makes a call to an external service - SNIP
            //var response = _someExternalService.PostHttpRequest(requestData);

            var response = new System3Response();
            response.Price = 92.67M;
            response.IsSuccess = true;
            response.Name = "zxcvbnm";
            response.Tax = 92.67M * 0.12M;

            return Task.FromResult(response);
        }
    }

    internal class System3Response
    {
        internal decimal Price { get; set; }
        internal bool IsSuccess { get; set; }
        internal string Name { get; set; }
        internal decimal Tax { get; set; }
    }
}
