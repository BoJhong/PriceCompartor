using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using PriceCompartor.Infrastructure;
using PriceCompartor.Models;

namespace PriceCompartor.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;

        public ProductController(ApplicationDbContext context, IMemoryCache memoryCache)
        {
            _context = context;
            _cache = memoryCache;
        }

        public IActionResult Index()
        {
            var products = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Platform);
            return View(products);
        }

        [HttpGet]
        public IActionResult Create()
        {
            LoadCategoriesAndPlatforms();
            return View();
        }

        [NonAction]
        private void LoadCategoriesAndPlatforms()
        {
            var categories = _context.Categories.ToList();
            ViewBag.Categories = new SelectList(categories, "Id", "Name");
            var platforms = _context.Platforms.ToList();
            ViewBag.Platforms = new SelectList(platforms, "Id", "Name");
        }

        [NonAction]
        private void UnloadCategoriesAndPlatforms()
        {
            ModelState.Remove("Categories");
            ModelState.Remove("Platforms");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Product model)
        {
            if (ModelState.IsValid)
            {
                UnloadCategoriesAndPlatforms();
                _context.Products.Add(model);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            LoadCategoriesAndPlatforms();
            _cache.Remove("ProductCounts");
            return View();
        }

        [HttpGet]
        public IActionResult Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            LoadCategoriesAndPlatforms();
            var product = _context.Products.Find(id);
            if (product == null)
            {
                return NotFound();
            }
            _cache.Remove("ProductCounts");
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Product model)
        {
            if (ModelState.IsValid)
            {
                UnloadCategoriesAndPlatforms();

                var existingProduct = _context.Products.Find(model.Id);
                if (existingProduct == null)
                {
                    return NotFound();
                }

                model.TotalRatingCount = existingProduct.TotalRatingCount;
                model.TotalRating = existingProduct.TotalRating;
                model.Rating = existingProduct.Rating;

                _context.Entry(existingProduct).CurrentValues.SetValues(model);

                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            LoadCategoriesAndPlatforms();
            _cache.Remove("ProductCounts");
            return View();
        }

        [HttpGet]
        public IActionResult Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            LoadCategoriesAndPlatforms();
            var product = _context.Products.Find(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(Product model)
        {
            ModelState.Remove("Categories");
            _context.Products.Remove(model);
            _context.SaveChanges();
            _cache.Remove("ProductCounts");
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Purge()
        {
            return View();
        }

        [HttpPost, ActionName("Purge")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PurgeConfirmed()
        {
            var allProducts = await _context.Products.ToListAsync();
            _context.Products.RemoveRange(allProducts);
            await _context.SaveChangesAsync();
            _cache.Remove("ProductCounts");
            return RedirectToAction(nameof(Index));
        }
    }
}
