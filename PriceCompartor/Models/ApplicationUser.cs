using Microsoft.AspNetCore.Identity;

namespace PriceCompartor.Models
{
    public class ApplicationUser : IdentityUser
    {
        public required string FirstName { get; set; }

        public required string LastName { get; set; }

        public required string Country { get; set; }

        public string? City { get; set; }
    }
}
