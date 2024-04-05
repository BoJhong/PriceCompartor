using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PriceCompartor.Data;
using PriceCompartor.Models;

namespace PriceCompartor.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var products = _context.Products.Include("Categories");
            return View(products);
        }

        [HttpGet]
        public IActionResult Create()
        {
            LoadCategories();
            return View();
        }

        [NonAction]
        private void LoadCategories()
        {
            var categories = _context.Categories.ToList();
            ViewBag.Categories = new SelectList(categories, "Id", "Name");
        }

        [HttpPost]
        public IActionResult Create(Product model)
        {
            if (ModelState.IsValid)
            {
                ModelState.Remove("Categories");
                _context.Products.Add(model);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            LoadCategories();
            return View();
        }

        [HttpGet]
        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            LoadCategories();
            var product = _context.Products.Find(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        [HttpPost]
        public IActionResult Edit(Product model)
        {
            if (ModelState.IsValid)
            {
                ModelState.Remove("Categories");
                _context.Products.Update(model);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            LoadCategories();
            return View();
        }

        [HttpGet]
        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            LoadCategories();
            var product = _context.Products.Find(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(Product model)
        {
            ModelState.Remove("Categories");
            _context.Products.Remove(model);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
    }
}
