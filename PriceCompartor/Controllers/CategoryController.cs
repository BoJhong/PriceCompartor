using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PriceCompartor.Infrastructure;
using PriceCompartor.Models;
using PriceCompartor.Models.ViewModels;

namespace PriceCompartor.Controllers
{
    public class CategoryController(ApplicationDbContext context) : Controller
    {
        private readonly ApplicationDbContext _context = context;

        public IActionResult Index(
            bool filterIsValid,
            FilterViewModel model,
            string[] filter,
            int? id,
            string? find,
            int page = 1
        )
        {
            if (filterIsValid)
            {
                model.PlatformCheckboxes = _context.Platforms.Select(p => new SelectListItem { Text = p.Name, Value = p.Id.ToString(), Selected = filter.Contains(p.Id.ToString()) }).ToList();
                HttpContext.Session.SetJson("Filter", model);
            }

            return View(GetProducts(id, find, page));
        }

        [NonAction]
        private IQueryable<Product> GetProducts(int? id, string? find, int page = 1)
        {
            ViewData["Categories"] = _context.Categories.ToList();
            ViewData["SelectedCategory"] = id;

            IQueryable<Product> products = _context.Products;

            if (find != null)
            {
                products = products.Where(p => p.Name.Contains(find));
            }

            if (id.HasValue && id != 0)
            {
                products = products.Where(p => p.CategoryId == id);
            }
            else if (id == 0)
            {
                products = products.Where(p => p.CategoryId == null);
            }

            const int pageSize = 60;

            ProductFilter productFilter = new ProductFilter(_context);

            products = productFilter.Filter(HttpContext, products);

            if (page < 1) page = 1;

            var count = products.Count();
            var pager = new Pager(count, page, pageSize);

            int recSkip = (page - 1) * pageSize;

            var data = products.Skip(recSkip).Take(pager.PageSize);

            ViewBag.Pager = pager;

            return data;
        }
    }
}
