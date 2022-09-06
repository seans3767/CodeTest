using ConsoleApp1.Exceptions;
using ConsoleApp1.Models;
using ConsoleApp1.QuotationSystems;
using Moq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Xunit;

namespace ConsoleApp1.Tests
{
    public class PriceEngineTests
    {
        private IList<Mock<IQuotationSystem>> _mockSystems = new List<Mock<IQuotationSystem>>();

        [Fact]
        public void Constructor_WhenQuotationSystemProviderNull_ThenThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => new PriceEngine(quotationSystemProvider: null));
        }

        [Fact]
        public void GetPrice_WhenRequestNull_ThenThrowsException()
        {
            var priceEngine = SetupPriceEngine();

            Assert.ThrowsAsync<ArgumentNullException>(() => priceEngine.GetPrice(null));
        }

        [Fact]
        public void GetPrice_WhenRiskDataMissing_ThenInvalid()
        {
            var priceEngine = SetupPriceEngine();
            var request = new PriceRequest() { RiskData = null };

            Assert.ThrowsAsync<ValidationException>(() => priceEngine.GetPrice(request));
        }

        [Fact]
        public void GetPrice_WhenFirstNameMissing_ThenInvalid()
        {
            var priceEngine = SetupPriceEngine();
            var request = new PriceRequest() { RiskData = DefaultRiskData() };
            request.RiskData.FirstName = null;

            Assert.ThrowsAsync<ValidationException>(() => priceEngine.GetPrice(request));
        }

        [Fact]
        public void GetPrice_WhenLastNameMissing_ThenInvalid()
        {
            var priceEngine = SetupPriceEngine();
            var request = new PriceRequest() { RiskData = DefaultRiskData() };
            request.RiskData.LastName = null;

            Assert.ThrowsAsync<ValidationException>(() => priceEngine.GetPrice(request));
        }

        [Fact]
        public void GetPrice_WhenValueMissing_ThenInvalid()
        {
            var priceEngine = SetupPriceEngine();
            var request = new PriceRequest() { RiskData = DefaultRiskData() };
            request.RiskData.Value = null;

            Assert.ThrowsAsync<ValidationException>(() => priceEngine.GetPrice(request));
        }

        [Fact]
        public void GetPrice_WhenNoQuotationSystems_ThenInvalidOperation()
        {
            _mockSystems = new List<Mock<IQuotationSystem>>();
            var priceEngine = SetupPriceEngine();
            var request = new PriceRequest() { RiskData = DefaultRiskData() };

            Assert.ThrowsAsync<QuotationSystemException>(() => priceEngine.GetPrice(request));
        }

        [Fact]
        public void GetPrice_WhenNoQuotationSystemAcceptsRequest_ThenInvalidOperation()
        {
            _mockSystems = new List<Mock<IQuotationSystem>>();
            var mockSystem = new Mock<IQuotationSystem>();
            mockSystem.Setup(s => s.Accepts(It.IsAny<QuotationRequest>())).Returns(false);
            _mockSystems.Add(mockSystem);

            var priceEngine = SetupPriceEngine();
            var request = new PriceRequest() { RiskData = DefaultRiskData() };

            Assert.ThrowsAsync<QuotationSystemException>(() => priceEngine.GetPrice(request));
        }

        [Fact]
        public async void GetPrice_WhenQuotationSystemDoesNotAcceptRequest_ThenGetPriceNotCalled()
        {
            _mockSystems = new List<Mock<IQuotationSystem>>();

            var mockSystem1 = new Mock<IQuotationSystem>();
            mockSystem1.Setup(s => s.Accepts(It.IsAny<QuotationRequest>())).Returns(false);
            _mockSystems.Add(mockSystem1);

            var mockSystem2 = new Mock<IQuotationSystem>();
            mockSystem2.Setup(s => s.Accepts(It.IsAny<QuotationRequest>())).Returns(true);
            _mockSystems.Add(mockSystem2);

            var priceEngine = SetupPriceEngine();
            var request = new PriceRequest() { RiskData = DefaultRiskData() };

            try
            {
                _ = await priceEngine.GetPrice(request);
            }
            catch (Exception) { }

            mockSystem1.Verify(s => s.GetPrice(It.IsAny<QuotationRequest>()), Times.Never());
            mockSystem2.Verify(s => s.GetPrice(It.IsAny<QuotationRequest>()), Times.Once());
        }

        [Fact]
        public async void GetPrice_WhenOnlyOneQuote_ThenIsReturned()
        {
            var quote = new QuotationResponse() { Price = 10, Tax = 1, Insurer = "Insurer" };
            _mockSystems = new List<Mock<IQuotationSystem>>();
            _mockSystems.Add(MockSystemForQuote(quote));

            var priceEngine = SetupPriceEngine();
            var request = new PriceRequest() { RiskData = DefaultRiskData() };

            var price = await priceEngine.GetPrice(request);

            Assert.Equal(quote.Price, price.Price);
            Assert.Equal(quote.Tax, price.Tax);
            Assert.Equal(quote.Insurer, price.InsurerName);
        }

        [Fact]
        public async void GetPrice_WhenMatchingQuotes_ThenOneIsReturned()
        {
            var quote1 = new QuotationResponse() { Price = 10, Tax = 1, Insurer = "Insurer 1" };
            var quote2 = new QuotationResponse() { Price = 10, Tax = 2, Insurer = "Insurer 2" };

            _mockSystems = new List<Mock<IQuotationSystem>>();
            _mockSystems.Add(MockSystemForQuote(quote1));
            _mockSystems.Add(MockSystemForQuote(quote2));

            var priceEngine = SetupPriceEngine();
            var request = new PriceRequest() { RiskData = DefaultRiskData() };

            var price = await priceEngine.GetPrice(request);

            Assert.Equal(10, price.Price);
        }

        [Fact]
        public async void GetPrice_WhenQuotationSystemFails_ThenIgnored()
        {
            var lowerQuote = new QuotationResponse() { Price = 10, Tax = 1, Insurer = "Insurer 1" };
            var higherQuote = new QuotationResponse() { Price = 20, Tax = 2, Insurer = "Insurer 2" };

            _mockSystems = new List<Mock<IQuotationSystem>>();
            _mockSystems.Add(MockSystemForQuote(higherQuote));

            var mockLowerQuoteSystem = MockSystemForQuote(lowerQuote);
            mockLowerQuoteSystem.Setup(s => s.GetPrice(It.IsAny<QuotationRequest>())).ThrowsAsync(new Exception("Request for lower quote failed!"));
            _mockSystems.Add(mockLowerQuoteSystem);

            var priceEngine = SetupPriceEngine();
            var request = new PriceRequest() { RiskData = DefaultRiskData() };

            var price = await priceEngine.GetPrice(request);

            Assert.Equal(higherQuote.Price, price.Price);
            Assert.Equal(higherQuote.Tax, price.Tax);
            Assert.Equal(higherQuote.Insurer, price.InsurerName);
        }

        [Fact]
        public async void GetPrice_WhenQuotationSystemReturnsNull_ThenIgnored()
        {
            var quote = new QuotationResponse() { Price = 20, Tax = 2, Insurer = "Insurer 2" };

            _mockSystems = new List<Mock<IQuotationSystem>>();
            _mockSystems.Add(MockSystemForQuote(quote));
            _mockSystems.Add(MockSystemForQuote(null));

            var priceEngine = SetupPriceEngine();
            var request = new PriceRequest() { RiskData = DefaultRiskData() };

            var price = await priceEngine.GetPrice(request);

            Assert.Equal(quote.Price, price.Price);
            Assert.Equal(quote.Tax, price.Tax);
            Assert.Equal(quote.Insurer, price.InsurerName);
        }

        [Fact]
        public void GetPrice_WhenNoQuoteReturned_ThenThrowsException()
        {
            _mockSystems = new List<Mock<IQuotationSystem>>();
            _mockSystems.Add(MockSystemForQuote(null));

            var priceEngine = SetupPriceEngine();
            var request = new PriceRequest() { RiskData = DefaultRiskData() };

            Assert.ThrowsAsync<QuotationException>(() => priceEngine.GetPrice(request));
        }

        [Fact]
        public async void GetPrice_WhenMultipleQuotes_ThenReturnsLowest()
        {
            var lowestQuote = new QuotationResponse() { Price = 10, Tax = 1, Insurer = "Insurer 1" };
            var quote2 = new QuotationResponse() { Price = 11, Tax = 1, Insurer = "Insurer 2" };
            var quote3 = new QuotationResponse() { Price = 20, Tax = 1, Insurer = "Insurer 3" };
            var quote4 = new QuotationResponse() { Price = 1000, Tax = 1, Insurer = "Insurer 4" };
            var quote5 = new QuotationResponse() { Price = 1234, Tax = 1, Insurer = "Insurer 5" };

            _mockSystems = new List<Mock<IQuotationSystem>>();
            _mockSystems.Add(MockSystemForQuote(lowestQuote));
            _mockSystems.Add(MockSystemForQuote(quote2));
            _mockSystems.Add(MockSystemForQuote(quote3));
            _mockSystems.Add(MockSystemForQuote(quote4));
            _mockSystems.Add(MockSystemForQuote(quote5));

            var priceEngine = SetupPriceEngine();
            var request = new PriceRequest() { RiskData = DefaultRiskData() };

            var price = await priceEngine.GetPrice(request);

            Assert.Equal(lowestQuote.Price, price.Price);
            Assert.Equal(lowestQuote.Tax, price.Tax);
            Assert.Equal(lowestQuote.Insurer, price.InsurerName);
        }

        private PriceEngine SetupPriceEngine()
        {
            var mockProvider = new Mock<IQuotationSystemProvider>();
            mockProvider.Setup(p => p.GetAll()).Returns(_mockSystems.Select(s => s.Object));
            var priceEngine = new PriceEngine(mockProvider.Object);
            return priceEngine;
        }

        private RiskData DefaultRiskData()
        {
            return new RiskData()
            {
                FirstName = "First name",
                LastName = "Last name",
                Value = 1000,
                Make = "Make",
                DOB = DateTime.Parse("1980-01-01")
            };
        }

        private Mock<IQuotationSystem> MockSystemForQuote(QuotationResponse? quote)
        {
            var mockSystem = new Mock<IQuotationSystem>();
            mockSystem.Setup(s => s.Accepts(It.IsAny<QuotationRequest>())).Returns(true);
            mockSystem.Setup(s => s.GetPrice(It.IsAny<QuotationRequest>()))
                .ReturnsAsync(quote);
            return mockSystem;
        }
    }
}