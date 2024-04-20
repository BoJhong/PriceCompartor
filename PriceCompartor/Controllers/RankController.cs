using Microsoft.AspNetCore.Mvc;
using PriceCompartor.Infrastructure;
using PriceCompartor.Models;

namespace PriceCompartor.Controllers
{
    [ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Any)]
    public class RankController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RankController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            if (_context.Products.Count() == 0) return View(new List<Product>());

            // 取得銷量和評分人數的最大值
            var maxSales = _context.Products.Max(p => p.Sales);
            var maxRatingCount = _context.Products.Max(p => p.TotalRatingCount);

            // 避免除以 0
            if (maxSales == 0) maxSales = 1;
            if (maxRatingCount == 0) maxRatingCount = 1;

            var topProducts = _context.Products
                .OrderByDescending(p => (p.Rating == 0 ? 3 : p.Rating) / 5 * 0.5 + p.Sales / maxSales * 0.3 + p.TotalRatingCount / maxRatingCount * 0.2) // 依照評分、評分人數和銷量進行排序（無人評分則當作3分）
                .Take(10)
                .ToList();

            return View(topProducts);
        }
    }
}
