using Microsoft.AspNetCore.Identity;

namespace PriceCompartor.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Country { get; set; }

        public string City { get; set; }
    }
}
