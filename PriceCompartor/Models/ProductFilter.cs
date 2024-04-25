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

        public List<Product> Filter(HttpContext httpContext, List<Product> products)
        {
            FilterViewModel filterOptions = httpContext.Session.GetJson<FilterViewModel>("Filter") ?? new FilterViewModel
            {
                PlatformCheckboxes = _context.Platforms.Select(p => new SelectListItem { Text = p.Name, Value = p.Id.ToString(), Selected = true }).ToList()
            };

            if (filterOptions != null)
            {
                if (filterOptions.MinPrice != null)
                {
                    products = products.Where(p => p.Price >= filterOptions.MinPrice).ToList();
                }

                if (filterOptions.MaxPrice != null)
                {
                    products = products.Where(p => p.Price <= filterOptions.MaxPrice).ToList();
                }

                switch (filterOptions.SelectedSortOrder)
                {
                    case SortOrderType.PriceAsc:
                        products = products.OrderBy(p => p.Price).ToList();
                        break;
                    case SortOrderType.PriceDesc:
                        products = products.OrderByDescending(p => p.Price).ToList();
                        break;
                    case SortOrderType.NameAsc:
                        products = products.OrderBy(p => p.Name).ToList();
                        break;
                    case SortOrderType.NameDesc:
                        products = products.OrderByDescending(p => p.Name).ToList();
                        break;
                    case SortOrderType.RatingAsc:
                        products = products.OrderBy(p => p.Rating).ToList();
                        break;
                    case SortOrderType.RatingDesc:
                        products = products.OrderByDescending(p => p.Rating).ToList();
                        break;
                    case SortOrderType.SalesAsc:
                        products = products.OrderBy(p => p.Sales).ToList();
                        break;
                    case SortOrderType.SalesDesc:
                        products = products.OrderByDescending(p => p.Sales).ToList();
                        break;
                }

                if (filterOptions.PlatformCheckboxes != null)
                {
                    products = products.Where(p => filterOptions.PlatformCheckboxes.Any(pc => p.PlatformId.ToString() == pc.Value && pc.Selected)).ToList();
                }
            }

            return products;
        }
    }
}
