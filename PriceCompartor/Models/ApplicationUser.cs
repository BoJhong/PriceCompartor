using Microsoft.AspNetCore.Identity;
using PriceCompartor.Infrastructure.Validation;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace PriceCompartor.Models
{
    public class ApplicationUser : IdentityUser
    {
        [StringLength(20)]
        public required string Nickname { get; set; }

        public GenderType Gender { get; set; }

        public DateTime DOB { get; internal set; }

        public DateTime RegistrationDate { get; set; }

        public byte[]? AvatarImage { get; set; }

        [StringLength(10)]
        public string? ImageMimeType { get; set; }

        public async Task SetImageDataAsync(IFormFile file)
        {
            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                AvatarImage = memoryStream.ToArray();
                ImageMimeType = file.ContentType;
            }
        }

        public string AvatarImageSrc
        {
            get
            {
                if (AvatarImage == null || ImageMimeType == null)
                {
                    var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img/default_avatar.png");
                    using (var fs = new FileStream(path, FileMode.Open))
                    {
                        AvatarImage = new byte[fs.Length];
                        fs.Read(AvatarImage, 0, AvatarImage.Length);
                        ImageMimeType = "image/png";
                    }
                }
                string mimeType = ImageMimeType;
                string base64 = Convert.ToBase64String(AvatarImage!);
                return $"data:{mimeType};base64,{base64}";
            }
        }

        [NotMapped]
        [FileExtension(["jpg", "jpeg", "png"])]
        [FileSize(1024 * 1024)]
        public IFormFile? AvatarImageUpload { get; set; }

        [InverseProperty("AppUser")]
        public List<Comment> Comments { get; set; } = new List<Comment>();
    }

    public enum GenderType
    {
        Male, Female, Unknown
    }
}
