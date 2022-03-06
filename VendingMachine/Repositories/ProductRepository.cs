using VendingMachine.Models;
using System.Resources;
using Microsoft.Extensions.Logging;
using VendingMachine.Services;
using VendingMachine.Data;

namespace VendingMachine.Repositories
{

    class ProductRepository : IProductRepository
    {
        private readonly string _currency = "C2";
        private readonly ITranslateService _translateService;
        private readonly ILogger<ProductRepository> _logger;
        private readonly IDataContext _context;

        public IEnumerable<Product> Products { get; set; }

        public ProductRepository(ITranslateService translateService, IDataContext context, ILogger<ProductRepository> logger)
        {
            _context = context;
            Products = _context.Products.AsReadOnly();
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

        public bool DisplayAvailableProductsByAmount(decimal amount)
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
    }
}