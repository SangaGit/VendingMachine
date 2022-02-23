namespace VendingMachine.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public double UnitPrice { get; set; }
        public int Quantity { get; set; }
    }
}