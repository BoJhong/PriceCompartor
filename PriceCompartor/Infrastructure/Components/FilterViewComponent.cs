using Microsoft.AspNetCore.Mvc;
using PriceCompartor.Models;
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
            FilterOptions filterOptions = HttpContext.Session.GetJson<FilterOptions>("Filter") ?? new(_context.Platforms.ToList());
            FilterViewModel filterVM = new()
            {
                FilterOptions = filterOptions,
                Products = _context.Products.ToList(),
            };

            return View(filterVM);
        }
    }
}
