namespace Magazin.Models
{
    public class CsvProductModel
    {
        public string ProductName { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string Currency { get; set; }
        public int StockQuantity { get; set; }
        public string CategoryName { get; set; }
        public string CategoryDescription { get; set; }
        public string ImageUrl { get; set; }
        public bool IsActive { get; set; }
    }

}
