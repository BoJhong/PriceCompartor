using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PriceCompartor.Models
{
    public class Product
    {
        [Key]
        public long Id { get; set; }

        public required string Link { get; set; }

        public string? ImageUrl { get; set; }

        public required string Name { get; set; }

        public string? Description { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "價格必須是正整數")]
        public required int Price { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "銷售量必須是正整數")]
        public int Sales { get; set; } = 0;

        public string? Address { get; set; }

        public string? OId { get; set; }

        public virtual List<Comment> Comments { get; set; } = new List<Comment>();

        // 所有評分的加總
        public long TotalRating { get; set; } = 0;

        // 評分的總數
        public long TotalRatingCount { get; set; } = 0;

        // 評分的平均值
        public float Rating { get; set; } = 0;

        [ForeignKey("Categories")]
        public int? CategoryId { get; set; }

        public virtual Category? Category { get; set; }

        [ForeignKey("Platforms")]
        public required int PlatformId { get; set; }

        public virtual Platform? Platform { get; set; }
    }
}
