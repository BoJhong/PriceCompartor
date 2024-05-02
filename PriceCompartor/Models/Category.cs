using PriceCompartor.Infrastructure.Validation;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PriceCompartor.Models
{
    public class Category
    {
        public int Id { get; set; }

        [DisplayName("Category")]
        public required string Name { get; set; }

        public byte[]? PhotoFile { get; set; }

        [StringLength(10)]
        public string? ImageMimeType { get; set; }

        public async Task SetImageDataAsync(IFormFile file)
        {
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            PhotoFile = memoryStream.ToArray();
            ImageMimeType = file.ContentType;
        }

        [NotMapped]
        [FileExtension(["jpg", "jpeg", "png"])]
        [FileSize(1024 * 1024)]
        public IFormFile? ImageUpload { get; set; }

        public virtual ICollection<Product>? Products { get; set; }
    }
}
