using Microsoft.AspNetCore.Mvc;
using PriceCompartor.Infrastructure;
using PriceCompartor.Models;
using PriceCompartor.Models.ViewModels;

namespace PriceCompartor.Controllers
{
    public class CartController(ApplicationDbContext context) : Controller
    {
        public IActionResult Index()
        {
            List<CartItem> cart = HttpContext.Session.GetJson<List<CartItem>>("Cart") ?? [];

            CartViewModel cartVM = new()
            {
                CartItems = cart,
                GrandTotal = cart.Sum(x => x.Quantity * x.Price)
            };

            return View(cartVM);
        }

        public IActionResult Add(long id)
        {
            Product? product = context.Products.Find(id);
            if (product == null)
            {
                return RedirectToAction("Index");
            }

            List<CartItem> cart = HttpContext.Session.GetJson<List<CartItem>>("Cart") ?? [];

            CartItem? cartItem = cart.FirstOrDefault(x => x.ProductId == id);
            if (cartItem == null)
            {
                cart.Add(new CartItem(product));
            }
            else
            {
                cartItem.Quantity++;
            }

            HttpContext.Session.SetJson("Cart", cart);

            TempData["Success"] = "Product added to cart successfully";

            return Redirect(Request.Headers["Referer"].ToString());
        }

        public IActionResult Decrease(long id)
        {
            List<CartItem> cart = HttpContext.Session.GetJson<List<CartItem>>("Cart") ?? [];

            CartItem? cartItem = cart.FirstOrDefault(x => x.ProductId == id);
            if (cartItem == null)
            {
                return RedirectToAction("Index");
            }
            if (cartItem.Quantity > 1)
            {
                cartItem.Quantity--;
            }
            else
            {
                cart.Remove(cartItem);
            }

            if (cart.Count == 0)
            {
                HttpContext.Session.Remove("Cart");
            }
            else
            {
                HttpContext.Session.SetJson("Cart", cart);
            }

            TempData["Success"] = "Product decreased from cart successfully";

            return RedirectToAction("Index");
        }

        public IActionResult Remove(long id)
        {
            List<CartItem> cart = HttpContext.Session.GetJson<List<CartItem>>("Cart") ?? [];

            CartItem? cartItem = cart.FirstOrDefault(x => x.ProductId == id);
            if (cartItem != null)
            {
                cart.Remove(cartItem);
            }

            if (cart.Count == 0)
            {
                HttpContext.Session.Remove("Cart");
            }
            else
            {
                HttpContext.Session.SetJson("Cart", cart);
            }

            TempData["Success"] = "Product removed from cart successfully";

            return RedirectToAction("Index");
        }

        public IActionResult Clear()
        {
            HttpContext.Session.Remove("Cart");

            TempData["Success"] = "Cart cleared successfully";

            return RedirectToAction("Index");
        }
    }
}
