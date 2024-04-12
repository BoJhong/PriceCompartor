using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PriceCompartor.Infrastructure;
using PriceCompartor.Models;

namespace PriceCompartor.Controllers
{
    public class CatController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CatController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(int id)
        {
            var products = _context.Products.Where(p => p.CategoryId == id).ToList();
            return View(products);
        }
    }
}
