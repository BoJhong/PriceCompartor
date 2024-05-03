using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Memory;
using PriceCompartor.Models.ViewModels;

namespace PriceCompartor.Infrastructure.Components
{
    public class FilterViewComponent(ApplicationDbContext context, IMemoryCache memoryCache) : ViewComponent
    {
        private readonly ApplicationDbContext _context = context;
        private readonly IMemoryCache _cache = memoryCache;

        public IViewComponentResult Invoke()
        {
            FilterViewModel? filterOptions = HttpContext.Session.GetJson<FilterViewModel>("Filter");

            // 如果沒有緩存的過濾選項，則使用預設選項
            if (filterOptions == null)
            {
                if (!_cache.TryGetValue("DefaultPlatformCheckboxes", out List<SelectListItem>? defaultPlatformCheckboxes))
                {
                    defaultPlatformCheckboxes = [.. _context.Platforms.Select(p => new SelectListItem { Text = p.Name, Value = p.Id.ToString(), Selected = true })];
                    _cache.Set("DefaultPlatformCheckboxes", defaultPlatformCheckboxes, TimeSpan.FromHours(1));
                }

                filterOptions = new FilterViewModel
                {
                    PlatformCheckboxes = defaultPlatformCheckboxes!
                };
            }

            ViewData["Categories"] = _context.Categories.ToList();

            return View(filterOptions);
        }
    }
}
