namespace PriceCompartor.Models.ViewModels
{
    public class CategoriesViewModel
    {
        public Dictionary<int, int>? ProductCounts { get; set; }

        public required List<Category> Categories { get; set; }
    }
}
