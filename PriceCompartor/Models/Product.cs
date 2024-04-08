using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PriceCompartor.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        public required string Link { get; set; }

        public string? ImageUrl { get; set; }

        public required string Name { get; set; }

        public string Description { get; set; } = string.Empty;

        public required uint Price { get; set; }

        public uint Sales { get; set; } = 0;

        public uint Rating { get; set; } = 0;

        public string? Address { get; set; }

        [ForeignKey("Categories")]
        public int? CategoryId { get; set; }

        public virtual Category? Categories { get; set; }

        [ForeignKey("Platforms")]
        public required int PlatformId { get; set; }

        public virtual Platform? Platforms { get; set; }
    }
}
