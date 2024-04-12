using System.ComponentModel.DataAnnotations;

namespace PriceCompartor.Infrastructure.Validation
{
    public class FileSizeAttribute : ValidationAttribute
    {
        private readonly int _maxSize;

        public FileSizeAttribute(int maxSize)
        {
            _maxSize = maxSize;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is IFormFile file)
            {
                if (file.Length > _maxSize)
                {
                    return new ValidationResult($"檔案大小不能超過 {_maxSize / 1024} KB。");
                }
            }

            return ValidationResult.Success;
        }
    }
}
