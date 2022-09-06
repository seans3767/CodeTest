using ConsoleApp1.Exceptions;
using ConsoleApp1.Models;
using ConsoleApp1.QuotationSystems;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    public class PriceEngine
    {
        private IQuotationSystemProvider _quotationSystemProvider;

        public PriceEngine(IQuotationSystemProvider quotationSystemProvider)
        {
            if (quotationSystemProvider == null)
            {
                throw new ArgumentNullException(nameof(quotationSystemProvider));
            }

            _quotationSystemProvider = quotationSystemProvider;
        }

        public async Task<PriceResponse> GetPrice(PriceRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            Validate(request);

            var quotes = await GetQuotes(request);

            PriceResponse response = null;

            if (!quotes.Any())
            {
                throw new QuotationException("Unable to retrieve quote for request");
            }

            var lowestQuote = quotes
                    .OrderBy(r => r.Price)
                    .FirstOrDefault();

            response = new PriceResponse()
            {
                Price = lowestQuote.Price,
                Tax = lowestQuote.Tax,
                InsurerName = lowestQuote.Insurer
            };

            return response;
        }

        private void Validate(PriceRequest request)
        {
            var validator = new DataAnnotationsValidator.DataAnnotationsValidator();
            var validationResults = new List<ValidationResult>();
            var isValid = validator.TryValidateObjectRecursive(request, validationResults);

            if (!isValid)
            {
                var errors = JsonConvert.SerializeObject(validationResults.Select(r => r.ErrorMessage));
                throw new ValidationException(errors);
            }
        }

        private async Task<IEnumerable<QuotationResponse>> GetQuotes(PriceRequest request)
        {
            var quotationRequest = new QuotationRequest()
            {
                FirstName = request.RiskData.FirstName,
                LastName = request.RiskData.LastName,
                Value = request.RiskData.Value.Value,
                Make = request.RiskData.Make,
                DOB = request.RiskData.DOB
            };

            var quotationSystems = GetAvailableQuotationSystemsForRequest(quotationRequest);

            IEnumerable<Task<QuotationResponse>> quotationTasks = null;

            try
            {
                quotationTasks = quotationSystems.Select(s => s.GetPrice(quotationRequest)).ToArray();
                await Task.WhenAll(quotationTasks);
            }
            catch (Exception)
            {
                // log
            }

            var successfulQuotes = quotationTasks
                .Where(t => t.Status == TaskStatus.RanToCompletion && t.Result != null)
                .Select(t => t.Result);

            return successfulQuotes;
        }

        private IEnumerable<IQuotationSystem> GetAvailableQuotationSystemsForRequest(QuotationRequest request)
        {
            var quotationSystems = _quotationSystemProvider.GetAll();

            if (!quotationSystems.Any())
            {
                throw new QuotationSystemException("No quotation systems available");
            }

            var validQuotationSystems = quotationSystems.Where(s => s.Accepts(request));

            if (!validQuotationSystems.Any())
            {
                throw new QuotationSystemException("No available quotation systems support this request");
            }

            return validQuotationSystems;
        }
    }
}
