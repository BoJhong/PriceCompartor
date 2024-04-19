using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Memory;
using PriceCompartor.Models.ViewModels;

namespace PriceCompartor.Infrastructure.Components
{
    public class FilterViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;

        public FilterViewComponent(ApplicationDbContext context, IMemoryCache memoryCache)
        {
            _context = context;
            _cache = memoryCache;
        }

        public IViewComponentResult Invoke()
        {
            FilterViewModel? filterOptions = HttpContext.Session.GetJson<FilterViewModel>("Filter");

            // 如果沒有緩存的過濾選項，則使用預設選項
            if (filterOptions == null)
            {
                if (!_cache.TryGetValue("DefaultPlatformCheckboxes", out List<SelectListItem>? defaultPlatformCheckboxes))
                {
                    defaultPlatformCheckboxes = _context.Platforms.Select(p => new SelectListItem { Text = p.Name, Value = p.Id.ToString(), Selected = true }).ToList();
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
