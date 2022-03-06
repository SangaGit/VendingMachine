using VendingMachine.Models;

namespace VendingMachine.Repositories
{
    public interface IProductRepository
    {
        public List<Product> Products { get; set; }

        public bool DisplayAvailableProductsByAmount(double amount);
        public void DisplayProducts();
        public void ReduceProductQuantity(Product product);
        public void SeedProductsDataCSV(string path);
    }
}