using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using PriceCompartor.Models.ViewModels;

namespace PriceCompartor.Infrastructure.Components
{
    public class CategoriesViewComponent(ApplicationDbContext context, IMemoryCache memoryCache) : ViewComponent
    {
        private readonly ApplicationDbContext _context = context;
        private readonly IMemoryCache _cache = memoryCache;

        public async Task<IViewComponentResult> InvokeAsync()
        {
            // 嘗試從快取中取得產品數量數據
            if (!_cache.TryGetValue("ProductCounts", out Dictionary<int, int>? productCounts))
            {
                // 如果快取中不存在產品數量數據，則從資料庫中查詢並快取數據
                productCounts = await _context.Products
                    .GroupBy(p => p.CategoryId ?? 0)
                    .Select(g => new { CategoryId = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.CategoryId, x => x.Count);

                productCounts[-1] = await _context.Products.CountAsync();

                // 設定快取項目並設定過期時間（這裡設定為1小時）
                _cache.Set("ProductCounts", productCounts, TimeSpan.FromHours(1));
            }

            var categories = await _context.Categories.Include(c => c.Products).ToListAsync();

            return View(new CategoriesViewModel {
                ProductCounts = productCounts,
                Categories = categories
            });
        }
    }
}
