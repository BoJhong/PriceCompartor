namespace PriceCompartor.Models
{
    public class CartItem
    {
        public long ProductId { get; set; }

        public string ProductName { get; set; }

        public uint Quantity { get; set; }

        public uint Price { get; set; }

        public uint Total
        {
            get { return Quantity * Price; }
        }

        public string? ImageUrl { get; set; }

        public CartItem() { }

        public CartItem(Product product)
        {
            ProductId = product.Id;
            ProductName = product.Name;
            Quantity = 1;
            Price = product.Price;
            ImageUrl = product.ImageUrl;
        }
    }
}
