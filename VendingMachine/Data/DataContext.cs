using VendingMachine.Models;

namespace VendingMachine.Data
{
    public class DataContext : IDataContext
    {
        public List<Product> Products { get; set; }

        public DataContext()
        {
            Products = SeedData();
        }
        private List<Product> SeedData()
        {
            return new List<Product>(){
                new Product(){Id = 1, Name = "COLA", UnitPrice = 1.00m, Quantity = 10},
                new Product(){Id = 2, Name = "Chips", UnitPrice = 0.50m, Quantity = 12},
                new Product(){Id = 3, Name = "Candy", UnitPrice = 0.65m, Quantity = 0}
            };
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