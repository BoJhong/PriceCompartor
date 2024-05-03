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
                model.PlatformCheckboxes = [.. context.Platforms.Select(p =>
                    new SelectListItem {
                        Text = p.Name,
                        Value = p.Id.ToString(),
                        Selected = filter.Contains(p.Id.ToString())
                    }
                )];

                HttpContext.Session.SetJson("Filter", model);
            }

            return View(GetProducts(id, find, page));
        }

        [NonAction]
        private IQueryable<Product> GetProducts(int? id, string? find, int page = 1)
        {
            ViewData["Categories"] = context.Categories.ToList();
            ViewData["SelectedCategory"] = id;

            IQueryable<Product> products = context.Products;

            if (find != null)
            {
                products = products.Where(p => p.Name.Contains(find));
            }

            if (id.HasValue)
            {
                products = id == 0
                    ? products.Where(p => p.CategoryId == null)
                    : products.Where(p => p.CategoryId == id);
            }

            ProductFilter productFilter = new(context);
            products = productFilter.Filter(HttpContext, products);

            // 計算商品總數量
            int count = products.Count();

            // 計算分頁
            const int pageSize = 60;
            int recSkip = (page < 1 ? 1 : page - 1) * pageSize;
            products = products.Skip(recSkip).Take(pageSize);

            // 創建 Pager 對象
            var pager = new Pager(count, page, pageSize);
            ViewBag.Pager = pager;

            return products;
        }
    }
}
