using Microsoft.AspNetCore.Mvc;
using PriceCompartor.Models;
using System.Diagnostics;

namespace PriceCompartor.Controllers
{
    public class HomeManagerController : Controller
    {
        private readonly ILogger<HomeManagerController> _logger;

        public HomeManagerController(ILogger<HomeManagerController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
