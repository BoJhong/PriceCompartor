
namespace PriceCompartor.Models
{
    public class FilterOptions
    {
        public List<Platform> Platforms { get; set; }

        public int MinPrice { get; set; }
        public int MaxPrice { get; set; }

        public OrderByType OrderBy { get; set; }

        public FilterOptions(List<Platform>? platforms)
        {
            Platforms = platforms ?? new List<Platform>();
            var shopee = Platforms.Find(Platforms => Platforms.Name == "Shopee");

            if (shopee != null) {
                shopee.filterEnabled = false;
            }

            MinPrice = 0;
            MaxPrice = int.MaxValue;
            OrderBy = OrderByType.PriceAsc;
        }
    }
}
