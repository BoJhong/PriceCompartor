using System.ComponentModel.DataAnnotations;

namespace PriceCompartor.Models
{
    public class ImageItem
    {
        public required byte[] Data { get; set; }

        [StringLength(10)]
        public required string MimeType { get; set; }
    }
}
