namespace PriceCompartor.Models.ViewModels
{
    public class CartViewModel
    {
        public required List<CartItem> CartItems { get; set; }

        public decimal GrandTotal { get; set; }
    }
}
