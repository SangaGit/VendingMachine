using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using VendingMachine.Models;
using System.Resources;
using Microsoft.Extensions.Logging;

namespace VendingMachine
{
    class ProductManager
    {
        private readonly string _currency = "C2";
        private readonly ResourceManager _rm;
        private readonly ILogger<ProductManager> _logger;
        public List<Product> Products { get; set; }

        public ProductManager(ResourceManager rm, ILogger<ProductManager> logger, params Product[] products)
        {
            Products = products.ToList();
            _rm = rm;
            _logger = logger;
        }

        public void DisplayProducts(){
            string? message = _rm.GetString("Items Left");
            Console.WriteLine(_rm.GetString("------LIST OF PRODUCTS------"));
            Console.WriteLine();
            foreach (var product in Products)
            {
                message = product.Quantity > 0 ? _rm.GetString("Items Left") : _rm.GetString("SOLD OUT");
                Console.WriteLine($"{product.Id}. {product.Name} {product.UnitPrice.ToString(_currency)} - {product.Quantity} {message}");
            }
            Console.WriteLine(_rm.GetString("Enter 4 to Quit...")); //use special character to quit when products grown
            Console.WriteLine();
        }

        public bool DisplayAvailableProductsByAmount(double amount){
            Console.WriteLine(_rm.GetString("------YOU CAN TRY DIFFERENT PRODUCT------"));
            Console.WriteLine();
            var products = Products.Where(x => x.Quantity > 0 && x.UnitPrice <= amount);
            if(products.Count() == 0) return false;
            foreach (var product in products)
            {
                Console.WriteLine($"{product.Id}. {product.Name} {product.UnitPrice.ToString(_currency)} - {product.Quantity} {_rm.GetString("Items Left")}");
            }
            Console.WriteLine(_rm.GetString("Enter 4 to Quit...")); //use special character to quit when products grown
            Console.WriteLine();
            return true;
        }

        public void ReduceProductQuantity(Product product){
            Product? prod = Products.FirstOrDefault(prod => prod.Id == product.Id);
            if(prod != null){
                prod.Quantity--;
            }
        }

        public void SeedProductsDataCSV(string path){
            Dictionary<int,int> csvData = new Dictionary<int, int>();
            try
            {
                string[] lines = System.IO.File.ReadAllLines(path);
                Product? prod;
                for(var i = 1 ; i < lines.Length; i++) //assume 1st row contain no data but headers only & column[0] = ProdId, column[1]=Stock
                {
                    string[] columns = lines[i].Split(',');
                    csvData.Add(int.Parse(columns[0]),int.Parse(columns[1]));
                }
                foreach (var item in csvData)
                {
                    prod = Products.Where(prod => prod.Id == item.Key).FirstOrDefault();
                    if(prod != null){
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