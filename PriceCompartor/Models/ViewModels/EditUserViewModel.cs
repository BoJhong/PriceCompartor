using Microsoft.AspNetCore.Mvc.Rendering;

namespace PriceCompartor.Models.ViewModels
{
    public class EditUserViewModel
    {
        public required ApplicationUser User { get; set; }
        public required IEnumerable<SelectListItem> Roles { get; set; }
    }
}
