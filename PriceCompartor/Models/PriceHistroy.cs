namespace PriceCompartor.Models
{
    public class PriceHistroy
    {
        public long Timestamp { get; set; }
        public DateTimeOffset DateTime
        {
            get
            {
                return DateTimeOffset.FromUnixTimeMilliseconds(Timestamp);
            }
        }
        public int Price { get; set; }
    }
}
