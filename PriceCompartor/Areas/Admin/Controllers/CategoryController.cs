using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PriceCompartor.Infrastructure;
using PriceCompartor.Models;

namespace PriceCompartor.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        public FileContentResult? GetImage(int id)
        {
            var photo = _context.Categories.Find(id);
            if (photo != null && photo.PhotoFile != null && photo.ImageMimeType != null)
            {
                return File(photo.PhotoFile, photo.ImageMimeType);
            }

            return null;
        }

        // GET: Category
        public async Task<IActionResult> Index()
        {
            return View(await _context.Categories.ToListAsync());
        }

        // GET: Category/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(m => m.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // GET: Category/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Category/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,ImageUpload")] Category category)
        {
            if (ModelState.IsValid)
            {
                if (category.ImageUpload != null)
                {
                    string[] allowedTypes = { "image/jpg", "image/jpeg", "image/png" };
                    if (!allowedTypes.Contains(category.ImageUpload.ContentType))
                    {
                        ModelState.AddModelError("PhotoFile", "只允許上傳 JPG、JPEG 和 PNG 格式的圖片。");
                        return View();
                    }
                    using (var stream = new MemoryStream())
                    {
                        await category.ImageUpload.CopyToAsync(stream);
                        category.PhotoFile = stream.ToArray();
                        category.ImageMimeType = category.ImageUpload.ContentType;
                    }
                }
                _context.Add(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // GET: Category/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        // POST: Category/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,ImageUpload")] Category category)
        {
            if (id != category.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                if (category.ImageUpload != null)
                {
                    string[] allowedTypes = { "image/jpg", "image/jpeg", "image/png" };
                    if (!allowedTypes.Contains(category.ImageUpload.ContentType))
                    {
                        ModelState.AddModelError("PhotoFile", "只允許上傳 JPG、JPEG 和 PNG 格式的圖片。");
                        return View();
                    }
                    await category.SetImageDataAsync(category.ImageUpload);
                }
                else
                { 
                    var existingCategory = await _context.Categories.FindAsync(id);
                    if (existingCategory != null)
                    {
                        category.PhotoFile = existingCategory.PhotoFile;
                        category.ImageMimeType = existingCategory.ImageMimeType;
                        _context.Entry(existingCategory).State = EntityState.Detached;
                    }
                }
                _context.Update(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // GET: Category/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(m => m.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // POST: Category/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                _context.Categories.Remove(category);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.Id == id);
        }
    }
}
