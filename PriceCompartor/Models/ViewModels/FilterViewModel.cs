
namespace PriceCompartor.Models.ViewModels
{
    public class FilterViewModel
    {
        public required FilterOptions FilterOptions { get; set; }

        public required List<Product> Products { get; set; }
    }
}
