using VendingMachine.Models;
using System.Resources;
using Microsoft.Extensions.Logging;
using VendingMachine.Services;

namespace VendingMachine.Repositories
{

    class ProductRepository : IProductRepository
    {
        private readonly string _currency = "C2";
        private readonly ITranslateService _translateService;
        private readonly ILogger<IProductRepository> _logger;
        public List<Product> Products { get; set; }

        public ProductRepository(ITranslateService translateService, ILogger<ProductRepository> logger, params Product[] products)
        {
            Products = products.ToList();
            _translateService = translateService;
            _logger = logger;
        }

        public void DisplayProducts()
        {
            string? message = _translateService.Translate("Items Left");
            Console.WriteLine(_translateService.Translate("------LIST OF PRODUCTS------"));
            Console.WriteLine();
            foreach (var product in Products)
            {
                message = product.Quantity > 0 ? _translateService.Translate("Items Left") : _translateService.Translate("SOLD OUT");
                Console.WriteLine($"{product.Id}. {product.Name} {product.UnitPrice.ToString(_currency)} - {product.Quantity} {message}");
            }
            Console.WriteLine(_translateService.Translate("Enter 4 to Quit...")); //use special character to quit when products grown
            Console.WriteLine();
        }

        public bool DisplayAvailableProductsByAmount(double amount)
        {
            Console.WriteLine(_translateService.Translate("------YOU CAN TRY DIFFERENT PRODUCT------"));
            Console.WriteLine();
            var products = Products.Where(x => x.Quantity > 0 && x.UnitPrice <= amount);
            if (products.Count() == 0) return false;
            foreach (var product in products)
            {
                Console.WriteLine($"{product.Id}. {product.Name} {product.UnitPrice.ToString(_currency)} - {product.Quantity} {_translateService.Translate("Items Left")}");
            }
            Console.WriteLine(_translateService.Translate("Enter 4 to Quit...")); //use special character to quit when products grown
            Console.WriteLine();
            return true;
        }

        public void ReduceProductQuantity(Product product)
        {
            Product? prod = Products.FirstOrDefault(prod => prod.Id == product.Id);
            if (prod != null)
            {
                prod.Quantity--;
            }
        }

        public void SeedProductsDataCSV(string path)
        {
            Dictionary<int, int> csvData = new Dictionary<int, int>();
            try
            {
                string[] lines = System.IO.File.ReadAllLines(path);
                Product? prod;
                for (var i = 1; i < lines.Length; i++) //assume 1st row contain no data but headers only & column[0] = ProdId, column[1]=Stock
                {
                    string[] columns = lines[i].Split(',');
                    csvData.Add(int.Parse(columns[0]), int.Parse(columns[1]));
                }
                foreach (var item in csvData)
                {
                    prod = Products.Where(prod => prod.Id == item.Key).FirstOrDefault();
                    if (prod != null)
                    {
                        prod.Quantity += item.Value;
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}