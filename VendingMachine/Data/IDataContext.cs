using VendingMachine.Models;

namespace VendingMachine.Data
{
    public interface IDataContext
    {
        public List<Product> Products { get; set; }

        public void SeedProductsDataCSV(string path);
    }
}