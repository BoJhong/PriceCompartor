using Azure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PriceCompartor.Infrastructure;
using PriceCompartor.Models;
using PriceCompartor.Utilities;

namespace PriceCompartor.Controllers
{
    public class SearchController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly WebCrawler _webCrawler;

        public SearchController(ApplicationDbContext context)
        {
            _context = context;
            _webCrawler = new WebCrawler(context);
        }

        public FileContentResult? GetPlatformImage(int id)
        {
            var photo = _context.Platforms.Find(id);
            if (photo != null && photo.PhotoFile != null && photo.ImageMimeType != null)
            {
                return File(photo.PhotoFile, photo.ImageMimeType);
            }

            return null;
        }

        public async Task<IActionResult> Index(string? keyword, int page = 1)
        {
            ViewData["keyword"] = keyword;
            if (!string.IsNullOrEmpty(keyword))
            {
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

                const int pageSize = 100;
                if (page < 1) page = 1;

                var pager = new Pager(int.MaxValue, page, pageSize);

                int recSkip = (page - 1) * pageSize;

                this.ViewBag.Pager = pager;

                return View(products);
            }

            return View(new List<Product>());
        }
    }
}
