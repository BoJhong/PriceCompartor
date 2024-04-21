using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PriceCompartor.Models
{
    public class Comment
    {
        public required string Id { get; set; }

        [ForeignKey("Product")]
        public long ProductId { get; set; }

        public virtual Product? Product { get; set; }

        [ForeignKey("AppUser")]
        public required string AppUserId { get; set; }

        public virtual ApplicationUser? AppUser { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }

        [StringLength(1000)]
        public string? Content { get; set; }

        public DateTime Time { get; set; }
    }
}
