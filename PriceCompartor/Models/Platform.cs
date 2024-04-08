using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace PriceCompartor.Models
{
    public class Platform
    {
        public int Id { get; set; }

        [DisplayName("Platform")]
        public required string Name { get; set; }

        public byte[]? PhotoFile { get; set; }

        [StringLength(10)]
        public string? ImageMimeType { get; set; }

        public async Task SetImageDataAsync(IFormFile file)
        {
            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                PhotoFile = memoryStream.ToArray();
                ImageMimeType = file.ContentType;
            }
        }
    }
}
