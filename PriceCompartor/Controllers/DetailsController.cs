using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PriceCompartor.Infrastructure;
using PriceCompartor.Models.ViewModels;
using PriceCompartor.Utilities;

namespace PriceCompartor.Controllers
{
    public class DetailsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly WebCrawler _webCrawler;

        public DetailsController(ApplicationDbContext context)
        {
            _context = context;
            _webCrawler = new WebCrawler(context);
        }

        public async Task<IActionResult> Index(int? id)
        {
            if (id == null) return NotFound();

            var product = _context.Products.Include(p => p.Categories).Include(p => p.Platforms).FirstOrDefault(p => p.Id == id);

            if (product == null) return NotFound();

            var productVM = new ProductViewModel
            {
                Product = product,
                PriceHistory = null
            };

            if (product.OId != null)
            {
                productVM.PriceHistory = await _webCrawler.GetPriceHistory(product.OId);
            }

            return View(productVM);
        }
    }
}
