namespace PriceCompartor.Models
{
    public enum OrderByType
    {
        PriceAsc,
        PriceDesc,
        DateAsc,
        DateDesc
    }

    public class OrderByTypeHelper
    {
        public static string GetDisplayName(OrderByType orderByType)
        {
            return orderByType switch
            {
                OrderByType.PriceAsc => "Price: Low to High",
                OrderByType.PriceDesc => "Price: High to Low",
                OrderByType.DateAsc => "Date: Old to New",
                OrderByType.DateDesc => "Date: New to Old",
                _ => "Unknown"
            };
        }
    }
}
