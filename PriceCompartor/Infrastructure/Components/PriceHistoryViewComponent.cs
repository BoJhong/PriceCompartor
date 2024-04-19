using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PriceCompartor.Models;

namespace PriceCompartor.Infrastructure.Components
{
    public class PriceHistoryViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;
        private readonly WebCrawler _webCrawler;

        public PriceHistoryViewComponent(ApplicationDbContext context)
        {
            _context = context;
            _webCrawler = new WebCrawler(context);
        }

        public async Task<IViewComponentResult> InvokeAsync(long productId)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null) return Content("Product not found");

            if (product.OId == null) return Content("Product not found on the platform");

            List<PriceHistroy> priceHistory = await _webCrawler.GetPriceHistory(product.OId);

            return View(priceHistory);
        }
    }
}
