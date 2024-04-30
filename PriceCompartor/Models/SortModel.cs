namespace PriceCompartor.Models
{
    public class SortModel
    {
        public required int CategoryId { get; set; }
        public required string CategoryName { get; set; }
        public required long ProductId { get; set; }
        public required string ProductName { get; set; }
    }
}
