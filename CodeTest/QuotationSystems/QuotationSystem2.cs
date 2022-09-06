using ConsoleApp1.Models;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;

namespace ConsoleApp1.QuotationSystems
{
    internal class QuotationSystem2 : IQuotationSystem
    {
        private const string Url = "http://quote-system-2.com";
        private const string Port = "1235";
        private readonly IEnumerable<string> _acceptedMakes = new List<string>() { "examplemake1", "examplemake2", "examplemake3" };

        public bool Accepts(QuotationRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            return _acceptedMakes.Contains(request.Make);
        }

        public async Task<QuotationResponse> GetPrice(QuotationRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (!_acceptedMakes.Contains(request.Make))
            {
                throw new ArgumentException("Unsupported make");
            }

            var system2Response = await SendRequest(request);

            if (!system2Response.HasPrice)
            {
                throw new Exception("Request to Quotation System 2 failed");
            }

            var response = new QuotationResponse()
            {
                Price = system2Response.Price,
                Tax = system2Response.Tax,
                Insurer = system2Response.Name
            };

            return response;
        }

        private Task<System2Response> SendRequest(QuotationRequest request)
        {
            //makes a call to an external service - SNIP
            //var response = _someExternalService.PostHttpRequest(requestData);

            var response = new System2Response();
            response.Price = 234.56M;
            response.HasPrice = true;
            response.Name = "qewtrywrh";
            response.Tax = 234.56M * 0.12M;

            return Task.FromResult(response);
        }
    }

    internal class System2Response
    {
        internal decimal Price { get; set; }
        internal bool HasPrice { get; set; }
        internal string Name { get; set; }
        internal decimal Tax { get; set; }
    }
}
