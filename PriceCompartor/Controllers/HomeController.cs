using Microsoft.AspNetCore.Mvc;
using PriceCompartor.Data;
using PriceCompartor.Models;
using System.Diagnostics;

namespace PriceCompartor.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
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

        public IActionResult Index(int page = 1)
        {
            ViewData["Categories"] = _context.Categories.ToList();
            List<Product> products = _context.Products.ToList();

            const int pageSize = 30;
            if (page < 1) page = 1;

            var pager = new Pager(products.Count(), page, pageSize);

            int recSkip = (page - 1) * pageSize;

            var data = products.Skip(recSkip).Take(pager.PageSize).ToList();

            this.ViewBag.Pager = pager;

            return View(data);
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
    }
}
