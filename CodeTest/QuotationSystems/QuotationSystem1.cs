using ConsoleApp1.Models;
using System;
using System.Dynamic;
using System.Threading.Tasks;

namespace ConsoleApp1.QuotationSystems
{
    internal class QuotationSystem1 : IQuotationSystem
    {
        private const string Url = "http://quote-system-1.com";
        private const string Port = "1234";

        public bool Accepts(QuotationRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            return request.DOB.HasValue;
        }

        public async Task<QuotationResponse> GetPrice(QuotationRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (!request.DOB.HasValue)
            {
                throw new ArgumentException("Date of birth not specified");
            }

            var system1Response = await SendRequest(request);

            if (!system1Response.IsSuccess)
            {
                throw new Exception("Request to Quotation System 1 failed");
            }

            var response = new QuotationResponse()
            {
                Price = system1Response.Price,
                Tax = system1Response.Tax,
                Insurer = system1Response.Name
            };

            return response;
        }

        private Task<System1Response> SendRequest(QuotationRequest request)
        {
            //makes a call to an external service - SNIP
            //var response = _someExternalService.PostHttpRequest(requestData);

            var response = new System1Response();
            response.Price = 123.45M;
            response.IsSuccess = true;
            response.Name = "Test Name";
            response.Tax = 123.45M * 0.12M;

            return Task.FromResult(response);
        }
    }

    internal class System1Response
    {
        internal decimal Price { get; set; }
        internal bool IsSuccess { get; set; }
        internal string Name { get; set; }
        internal decimal Tax { get; set; }
    }
}
