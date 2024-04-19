using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PriceCompartor.Infrastructure;

namespace PriceCompartor.Controllers
{
    public class DetailsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DetailsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(int? id)
        {
            if (id == null) return NotFound();

            var product = _context.Products.Include(p => p.Categories).Include(p => p.Platforms).FirstOrDefault(p => p.Id == id);

            if (product == null) return NotFound();

            return View(product);
        }
    }
}
