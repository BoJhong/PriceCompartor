using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using NuGet.Packaging;
using OpenAI_API;
using OpenAI_API.Chat;
using OpenAI_API.Completions;
using PriceCompartor.Infrastructure;
using PriceCompartor.Models;
using System.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace PriceCompartor.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;
        private readonly string? _apiKey;

        public ProductController(ApplicationDbContext context, IMemoryCache memoryCache)
        {
            _context = context;
            _cache = memoryCache;
            _apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        }

        public IActionResult Index(int page = 1)
        {
            var products = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Platform)
                .ToList();

            const int pageSize = 10;

            if (page < 1) page = 1;

            var pager = new Pager(products.Count(), page, pageSize);

            int recSkip = (page - 1) * pageSize;

            var data = products.Skip(recSkip).Take(pager.PageSize).ToList();

            ViewBag.Pager = pager;

            return View(data);
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

        [HttpGet]
        public IActionResult Sort()
        {
            return View();
        }

        [HttpPost, ActionName("Sort")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SortConfirmed()
        {
            GeminiTextRequest geminiTextRequest = new GeminiTextRequest();

            var uncategorizedProducts = _context.Products.Where(p => p.CategoryId == null).Select(p => new { p.Id, p.Name }).ToList();
            string categories = JsonConvert.SerializeObject(_context.Categories.Select(p => new { p.Id, p.Name }).ToList());

            while (uncategorizedProducts.Count > 0)
            {
                List<Task> tasks = new List<Task>();

                int takeCount = Math.Min(10, uncategorizedProducts.Count());
                if (takeCount == 0) break;
                string products = JsonConvert.SerializeObject(uncategorizedProducts.Take(takeCount).ToList());
                uncategorizedProducts.RemoveRange(0, takeCount);

                string prompt = string.Format(@"
                Please help me categorize all my products.

                Categories:
                {0}

                Products:
                {1}


                IMPORTANT: The output should be a JSON array of multiple titles without field names. Just the titles! Make Sure the JSON is valid.
                
                ", categories, products);

                prompt += @"
                Example Output:
                [
                    { CategoryId: ""CategoryId"", CategoryName: ""CategoryName"", ProductId: ""ProductId"", ProductName: ""ProductName"" },
                    { CategoryId: ""CategoryId"", CategoryName: ""CategoryName"", ProductId: ""ProductId"", ProductName: ""ProductName"" },
                    { CategoryId: ""CategoryId"", CategoryName: ""CategoryName"", ProductId: ""ProductId"", ProductName: ""ProductName"" }
                ]
                ";

                GeminiTextResponse geminiTextResponse = await geminiTextRequest.SendMsg(prompt);

                if (geminiTextResponse == null) continue;

                string? responseText = geminiTextResponse.candidates?[0].content.parts[0].text;

                if (responseText == null) continue;

                List<SortModel>? result = JsonExtractor.ExtractJson<SortModel>(responseText);

                if (result == null) continue;

                foreach (SortModel sortModel in result)
                {
                    var product = _context.Products.Find(sortModel.ProductId);
                    if (product == null) continue;
                    product.CategoryId = sortModel.CategoryId;
                    _context.Products.Update(product);
                }

                _context.SaveChanges();
                _cache.Remove("ProductCounts");
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

