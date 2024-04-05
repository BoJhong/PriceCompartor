using System.ComponentModel.DataAnnotations.Schema;

namespace PriceCompartor.Models
{
    public class Product
    {
        public int Id { get; set; }

        public required string Name { get; set; }

        public string Description { get; set; } = string.Empty;

        public required uint Price { get; set; }

        public required uint Quantity { get; set;}

        [ForeignKey("Categories")]
        public required int CategoryId { get; set; }

        public virtual Category? Categories { get; set; }
    }
}
