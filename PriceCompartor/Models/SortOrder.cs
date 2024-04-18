namespace PriceCompartor.Models
{
    public enum SortOrderType
    {
        NameAsc,
        NameDesc,
        PriceAsc,
        PriceDesc,
        RatingAsc,
        RatingDesc,
        SalesAsc,
        SalesDesc
    }

    public class SortOrder
    {
        public SortOrderType sortOrder { get; set; }

        public string DisplayName => GetDisplayName(sortOrder);

        public static string GetDisplayName(SortOrderType orderByType)
        {
            return orderByType switch
            {
                SortOrderType.NameAsc => "Name: A to Z",
                SortOrderType.NameDesc => "Name: Z to A",
                SortOrderType.PriceAsc => "Price: Low to High",
                SortOrderType.PriceDesc => "Price: High to Low",
                SortOrderType.RatingAsc => "Rating: Low to High",
                SortOrderType.RatingDesc => "Rating: High to Low",
                SortOrderType.SalesAsc => "Sales: Low to High",
                SortOrderType.SalesDesc => "Sales: High to Low",
                _ => "Unknown"
            };
        }
    }
}
