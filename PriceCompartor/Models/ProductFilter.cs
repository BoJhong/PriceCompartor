using Microsoft.AspNetCore.Mvc.Rendering;
using PriceCompartor.Infrastructure;
using PriceCompartor.Models.ViewModels;

namespace PriceCompartor.Models
{
    public class ProductFilter
    {
        private readonly ApplicationDbContext _context;

        public ProductFilter(ApplicationDbContext context)
        {
            _context = context;
        }

        public IQueryable<Product> Filter(HttpContext httpContext, IQueryable<Product> products)
        {
            FilterViewModel filterOptions = httpContext.Session.GetJson<FilterViewModel>("Filter") ?? new FilterViewModel
            {
                PlatformCheckboxes = _context.Platforms.Select(p => new SelectListItem { Text = p.Name, Value = p.Id.ToString(), Selected = true }).ToList()
            };

            if (filterOptions == null)
            {
                return products;
            }

            if (filterOptions.MinPrice != null)
            {
                products = products.Where(p => p.Price >= filterOptions.MinPrice);
            }

            if (filterOptions.MaxPrice != null)
            {
                products = products.Where(p => p.Price <= filterOptions.MaxPrice);
            }

            switch (filterOptions.SelectedSortOrder)
            {
                case SortOrderType.PriceAsc:
                    products = products.OrderBy(p => p.Price);
                    break;
                case SortOrderType.PriceDesc:
                    products = products.OrderByDescending(p => p.Price);
                    break;
                case SortOrderType.NameAsc:
                    products = products.OrderBy(p => p.Name);
                    break;
                case SortOrderType.NameDesc:
                    products = products.OrderByDescending(p => p.Name);
                    break;
                case SortOrderType.RatingAsc:
                    products = products.OrderBy(p => p.Rating);
                    break;
                case SortOrderType.RatingDesc:
                    products = products.OrderByDescending(p => p.Rating);
                    break;
                case SortOrderType.SalesAsc:
                    products = products.OrderBy(p => p.Sales);
                    break;
                case SortOrderType.SalesDesc:
                    products = products.OrderByDescending(p => p.Sales);
                    break;
            }

            if (filterOptions.PlatformCheckboxes != null && filterOptions.PlatformCheckboxes.Any(pc => pc.Selected))
            {
                var selectedPlatformIds = filterOptions.PlatformCheckboxes.Where(pc => pc.Selected).Select(pc => pc.Value);
                products = products.Where(p => selectedPlatformIds.Contains(p.PlatformId.ToString()));
            }

            return products;
        }
    }
}
