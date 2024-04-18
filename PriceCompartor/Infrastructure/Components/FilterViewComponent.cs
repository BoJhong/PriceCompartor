using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PriceCompartor.Models.ViewModels;

namespace PriceCompartor.Infrastructure.Components
{
    public class FilterViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public FilterViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public IViewComponentResult Invoke()
        {
            FilterViewModel filterOptions = HttpContext.Session.GetJson<FilterViewModel>("Filter") ?? new FilterViewModel
            {
                PlatformCheckboxes = _context.Platforms.Select(p => new SelectListItem { Text = p.Name, Value = p.Id.ToString(), Selected = true }).ToList()
            };

            ViewData["Categories"] = _context.Categories.ToList();

            return View(filterOptions);
        }
    }
}
