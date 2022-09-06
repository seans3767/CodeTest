using ConsoleApp1.Models;
using System;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            MainAsync().GetAwaiter().GetResult();
        }

        static async Task MainAsync()
        {
            //SNIP - collect input (risk data from the user)

            var request = new PriceRequest()
            {
                RiskData = new RiskData()
                {
                    DOB = DateTime.Parse("1980-01-01"),
                    FirstName = "John",
                    LastName = "Smith",
                    Make = "Cool New Phone",
                    Value = 500
                }
            };

            // In a real case this would be injected by the IoC container by configuring it as a service, but here the class is being instanciated directly
            var quotationSystemProvider = new QuotationSystems.QuotationSystemProvider();
            var priceEngine = new PriceEngine(quotationSystemProvider);

            try
            {
                var price = await priceEngine.GetPrice(request);
                Console.WriteLine(String.Format("You price is {0}, from insurer: {1}. This includes tax of {2}", price.Price, price.InsurerName, price.Tax));
            }
            catch (Exception ex)
            {
                Console.WriteLine(String.Format("There was an error - {0}", ex.Message));
            }

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }
    }
}
