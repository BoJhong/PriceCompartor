using System.ComponentModel.DataAnnotations;

namespace PriceCompartor.Infrastructure.Validation
{
    public class FileExtensionAttribute : ValidationAttribute
    {
        private readonly string[] _extensions;

        public FileExtensionAttribute(string[] extensions)
        {
            _extensions = extensions;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is IFormFile file)
            {
                string extension = Path.GetExtension(file.FileName);
                if (!_extensions.Any(x => extension.ToLower().EndsWith(x)))
                {
                    return new ValidationResult($"檔案格式必須為 {string.Join(", ", _extensions)}。");
                }
            }

            return ValidationResult.Success;
        }
    }
}
