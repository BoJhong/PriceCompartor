using Microsoft.AspNetCore.Mvc;
using PriceCompartor.Infrastructure;

namespace PriceCompartor.Controllers
{
    public class RankController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RankController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var topProducts = _context.Products
                .OrderByDescending(p => p.Sales)
                .Take(10)
                .ToList();

            return View(topProducts);
        }
    }
}
