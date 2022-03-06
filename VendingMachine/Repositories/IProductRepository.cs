using VendingMachine.Models;

namespace VendingMachine.Repositories
{
    public interface IProductRepository
    {
        public IEnumerable<Product> Products { get; set; }

        public bool DisplayAvailableProductsByAmount(decimal amount);
        public void DisplayProducts();
        public void ReduceProductQuantity(Product product);
    }
}