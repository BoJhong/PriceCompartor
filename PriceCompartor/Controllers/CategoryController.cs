using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PriceCompartor.Infrastructure;
using PriceCompartor.Models;
using PriceCompartor.Models.ViewModels;
using System.Diagnostics;

namespace PriceCompartor.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public CategoryController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index(int? id, int page = 1)
        {
            ViewData["Categories"] = _context.Categories.ToList();
            ViewData["SelectedCategory"] = id;

            List<Product> products = (
                id != null
                    ? id != 0
                        ? _context.Products.Where(p => p.CategoryId == id)
                        : _context.Products.Where(p => p.CategoryId == null)
                    : _context.Products
            )
            .Include(p => p.Categories)
            .Include(p => p.Platforms).ToList();

            const int pageSize = 60;
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
