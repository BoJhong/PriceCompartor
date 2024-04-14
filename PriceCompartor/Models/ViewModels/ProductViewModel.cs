namespace PriceCompartor.Models.ViewModels
{
    public class ProductViewModel
    {
        public required Product Product { get; set; }

        public required List<PriceHistroy>? PriceHistory { get; set; }
    }
}
