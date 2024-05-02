using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using PriceCompartor.Infrastructure;
using PriceCompartor.Models;

namespace PriceCompartor.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class PlatformController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;

        public PlatformController(ApplicationDbContext context, IMemoryCache memoryCache)
        {
            _context = context;
            _cache = memoryCache;
        }

        public FileContentResult? GetImage(int id)
        {
            var photo = _context.Platforms.Find(id);
            if (photo != null && photo.PhotoFile != null && photo.ImageMimeType != null)
            {
                return File(photo.PhotoFile, photo.ImageMimeType);
            }

            return null;
        }

        // GET: Platform
        public async Task<IActionResult> Index()
        {
            return View(await _context.Platforms.ToListAsync());
        }

        // GET: Platform/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var platform = await _context.Platforms
                .FirstOrDefaultAsync(m => m.Id == id);
            if (platform == null)
            {
                return NotFound();
            }

            return View(platform);
        }

        // GET: Platform/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Platform/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,ImageUpload")] Platform platform)
        {
            if (ModelState.IsValid)
            {
                if (platform.ImageUpload != null)
                {
                    string[] allowedTypes = { "image/jpeg", "image/png" };
                    if (!allowedTypes.Contains(platform.ImageUpload.ContentType))
                    {
                        ModelState.AddModelError("PhotoFile", "只允許上傳 JPG、JPEG 和 PNG 格式的圖片。");
                        return View();
                    }
                    await platform.SetImageDataAsync(platform.ImageUpload);
                }
                _context.Add(platform);
                await _context.SaveChangesAsync();
                _cache.Remove("DefaultPlatformCheckboxes");
                return RedirectToAction(nameof(Index));
            }
            return View(platform);
        }

        // GET: Platform/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var platform = await _context.Platforms.FindAsync(id);
            if (platform == null)
            {
                return NotFound();
            }
            return View(platform);
        }

        // POST: Platform/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,ImageUpload")] Platform platform)
        {
            if (id != platform.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                if (platform.ImageUpload != null)
                {
                    string[] allowedTypes = { "image/jpg", "image/jpeg", "image/png" };
                    if (!allowedTypes.Contains(platform.ImageUpload.ContentType))
                    {
                        ModelState.AddModelError("PhotoFile", "只允許上傳 JPG、JPEG 和 PNG 格式的圖片。");
                        return View();
                    }
                    await platform.SetImageDataAsync(platform.ImageUpload);
                }
                else
                {
                    var existingPlatform = await _context.Platforms.FindAsync(id);
                    if (existingPlatform != null)
                    {
                        platform.PhotoFile = existingPlatform.PhotoFile;
                        platform.ImageMimeType = existingPlatform.ImageMimeType;
                        _context.Entry(existingPlatform).State = EntityState.Detached;
                    }
                }
                _context.Update(platform);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(platform);
        }

        // GET: Platform/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var platform = await _context.Platforms
                .FirstOrDefaultAsync(m => m.Id == id);
            if (platform == null)
            {
                return NotFound();
            }

            return View(platform);
        }

        // POST: Platform/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var platform = await _context.Platforms.FindAsync(id);
            if (platform != null)
            {
                _context.Platforms.Remove(platform);
            }

            await _context.SaveChangesAsync();
            _cache.Remove("DefaultPlatformCheckboxes");
            return RedirectToAction(nameof(Index));
        }

        private bool PlatformExists(int id)
        {
            return _context.Platforms.Any(e => e.Id == id);
        }
    }
}
