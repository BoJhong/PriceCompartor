using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PriceCompartor.Infrastructure;
using PriceCompartor.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PriceCompartor.Controllers
{
    [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any)]
    public class DetailsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private UserManager<ApplicationUser> _userManager { get; }

        public DetailsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index(int? id)
        {
            if (id == null) return NotFound();

            var product = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Platform)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.AppUser)
                .FirstOrDefault(p => p.Id == id);

            if (product == null) return NotFound();

            product.Comments = product.Comments.OrderBy(c => c.Time).ToList();

            return View(product);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(int id, string content, int rating = 0)
        {
            if (rating < 1 || rating > 5) return BadRequest();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();
            var product = _context.Products.FirstOrDefault(p => p.Id == id);
            if (product == null) return NotFound();

            var comment = new Comment()
            {
                Id = Guid.NewGuid().ToString(),
                Time = DateTime.Now,
                Rating = rating,
                Content = content,
                ProductId = id,
                AppUserId = user.Id
            };
            _context.Add(comment);

            product.TotalRating += rating;
            product.TotalRatingCount++;
            product.Rating = product.TotalRatingCount != 0
                ? (float)product.TotalRating / product.TotalRatingCount
                : 0;
            
            // 捨去小數點第一位後面的數字
            product.Rating = (float)Math.Floor(product.Rating * 10) / 10;

            _context.Update(product);

            await _context.SaveChangesAsync();

            return RedirectToAction("Index", new { id });
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveComment(int id, string commentId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();
            var product = _context.Products.FirstOrDefault(p => p.Id == id);
            if (product == null) return NotFound();

            var comment = _context.Comments.FirstOrDefault(c => c.Id == commentId && c.ProductId == id);

            bool isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

            if (comment == null || (!isAdmin && comment.AppUserId != user.Id)) return NotFound();

            _context.Remove(comment);

            product.TotalRating -= comment.Rating;
            product.TotalRatingCount--;
            product.Rating = product.TotalRatingCount != 0
                ? product.TotalRating / product.TotalRatingCount
                : 0;

            _context.Update(product);

            await _context.SaveChangesAsync();

            return RedirectToAction("Index", new { id });
        }
    }
}
