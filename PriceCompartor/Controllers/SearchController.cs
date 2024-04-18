using Azure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using PriceCompartor.Infrastructure;
using PriceCompartor.Models;
using PriceCompartor.Models.ViewModels;
using PriceCompartor.Utilities;

namespace PriceCompartor.Controllers
{
    public class SearchController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;
        private readonly WebCrawler _webCrawler;

        public SearchController(ApplicationDbContext context, IMemoryCache memoryCache)
        {
            _context = context;
            _cache = memoryCache;
            _webCrawler = new WebCrawler(_context);
        }

        public IActionResult Index(bool filterIsValid, FilterViewModel model, string[] filter, string? keyword, int page = 1)
        {
            if (Request.Headers.ContainsKey("Referer"))
            {
                // 取得 Referer 標頭中的 URL
                string refererUrl = Request.Headers["Referer"].ToString();

                // 重新導向至上一頁
                return Redirect(refererUrl);
            }

            if (filterIsValid)
            {
                model.PlatformCheckboxes = _context.Platforms.Select(p => new SelectListItem { Text = p.Name, Value = p.Id.ToString(), Selected = filter.Contains(p.Id.ToString()) }).ToList();
                HttpContext.Session.SetJson("Filter", model);
            }

            return View(GetProducts(keyword, page).Result);
        }

        [NonAction]
        private async Task<List<Product>> GetProducts(string? keyword, int page = 1)
        {
            if (string.IsNullOrEmpty(keyword)) return new List<Product>();

            var products = await _webCrawler.GetProducts(keyword, page);
            foreach (var product in products)
            {
                var existingProduct = await _context.Products.FirstOrDefaultAsync(p => p.Link == product.Link);

                if (existingProduct == null)
                {
                    _context.Products.Add(product);
                }
                else
                {
                    _context.Products.Update(existingProduct);
                }
            }
            await _context.SaveChangesAsync();

            ProductFilter productFilter = new ProductFilter(_context);

            products = productFilter.Filter(HttpContext, products);

            const int pageSize = 100;
            if (page < 1) page = 1;

            var pager = new Pager(int.MaxValue, page, pageSize);

            int recSkip = (page - 1) * pageSize;

            ViewBag.Pager = pager;

            // 清除分類數量的緩存
            _cache.Remove("ProductCounts");

            return products;
        }
    }
}
