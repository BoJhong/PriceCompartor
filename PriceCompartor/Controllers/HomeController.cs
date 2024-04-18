using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using PriceCompartor.Infrastructure;
using PriceCompartor.Models;
using PriceCompartor.Models.ViewModels;
using System.Diagnostics;

namespace PriceCompartor.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, IMemoryCache memoryCache)
        {
            _logger = logger;
            _context = context;
            _cache = memoryCache;
        }

        public IActionResult Index()
        {
            ViewData["Categories"] = _context.Categories.ToList();

            List<Product> products = _context.Products
                .OrderBy(p => Guid.NewGuid())
                .Take(60)
                .Include(p => p.Categories)
                .Include(p => p.Platforms).ToList();

            return View(products);
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
            // ���ձq�֨������o�Ϥ����
            if (_cache.TryGetValue($"PlatformImage_{id}", out ImageItem? cachedItem))
            {
                if (cachedItem?.Data == null || cachedItem.MimeType == null)
                {
                    return null;
                }
                return File(cachedItem.Data, cachedItem.MimeType);
            }

            var photo = _context.Platforms.Find(id);
            if (photo != null && photo.PhotoFile != null && photo.ImageMimeType != null)
            {
                // �N�Ϥ���Ʀs�J�֨��A���Ĵ����� 10 ����
                var cacheItem = new ImageItem
                {
                    Data = photo.PhotoFile,
                    MimeType = photo.ImageMimeType
                };
                _cache.Set($"PlatformImage_{id}", cacheItem, TimeSpan.FromMinutes(10));
                return File(photo.PhotoFile, photo.ImageMimeType);
            }

            return null;
        }
    }
}
