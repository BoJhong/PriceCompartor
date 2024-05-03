using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using PriceCompartor.Infrastructure;
using PriceCompartor.Models;
using PriceCompartor.Models.ViewModels;
using System.Diagnostics;

namespace PriceCompartor.Controllers
{
    [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any)]
    public class HomeController(ApplicationDbContext context, IMemoryCache cache) : Controller
    {
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Index()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> GetMoreProducts()
        {
            int productCount;
            if (cache.TryGetValue("ProductCounts", out Dictionary<int, int>? productCounts))
            {
                productCount = productCounts![-1];
            } else
            {
                productCount = await context.Products.CountAsync();
            }

            List<Product> products = [.. context.Products
                .OrderBy(p => Guid.NewGuid())
                .Take(30)
                .Include(p => p.Category)
                .Include(p => p.Platform)];

            return PartialView("_ProductCardPartial", products);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public FileContentResult? GetPlatformImage(int id)
        {
            var photo = context.Platforms.Find(id);
            if (photo != null && photo.PhotoFile != null && photo.ImageMimeType != null)
            {
                return File(photo.PhotoFile, photo.ImageMimeType);
            }

            return null;
        }

        public FileContentResult? GetCategoryImage(int id)
        {
            var photo = context.Categories.Find(id);
            if (photo != null && photo.PhotoFile != null && photo.ImageMimeType != null)
            {
                return File(photo.PhotoFile, photo.ImageMimeType);
            }

            return null;
        }
    }
}
