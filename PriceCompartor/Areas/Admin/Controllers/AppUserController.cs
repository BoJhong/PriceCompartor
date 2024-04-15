using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using PriceCompartor.Models;
using PriceCompartor.Models.ViewModels;

namespace PriceCompartor.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AppUserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AppUserController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            List<ApplicationUserViewModel> userViewModels = new List<ApplicationUserViewModel>();
            var allUsers = _userManager.Users.ToList();
            foreach (var user in allUsers)
            {
                userViewModels.Add(new ApplicationUserViewModel
                {
                    User = user,
                    RoleName = string.Join(", ", await _userManager.GetRolesAsync(user))
                });
            }
            return View(userViewModels);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ApplicationUser identityUser)
        {
            if (ModelState.IsValid)
            {
                _userManager.CreateAsync(identityUser).GetAwaiter().GetResult();
                return RedirectToAction("Index");
            }
            return View();
        }

        [HttpGet]
        public IActionResult Edit(string id)
        {
            var user = _userManager.FindByIdAsync(id).GetAwaiter().GetResult();
            if (user == null) return NotFound();
            var userRoles = _userManager.GetRolesAsync(user).GetAwaiter().GetResult();
            var allRoles = _roleManager.Roles.ToList();
            List<SelectListItem> roleList = new List<SelectListItem>();
            foreach (var role in allRoles)
            {
                roleList.Add(new SelectListItem
                {
                    Text = role.Name,
                    Value = role.NormalizedName,
                    Selected = userRoles.Contains(role.Name!)
                });
            }
            ViewBag.Roles = roleList;
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(string id, List<string> roles)
        {
            var user = _userManager.FindByIdAsync(id).GetAwaiter().GetResult();
            if (user == null) return NotFound();
            var userRoles = _userManager.GetRolesAsync(user).GetAwaiter().GetResult();
            var result = _userManager.RemoveFromRolesAsync(user, userRoles).GetAwaiter().GetResult();
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "無法移除使用者的角色");
                return View(user);
            }
            result = _userManager.AddToRolesAsync(user, roles).GetAwaiter().GetResult();
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "無法新增使用者的角色");
                return View(user);
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Delete(string id)
        {
            var user = _userManager.FindByIdAsync(id).GetAwaiter().GetResult();
            if (user == null) return NotFound();
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(string id)
        {
            var user = _userManager.FindByIdAsync(id).GetAwaiter().GetResult();
            if (user != null)
            {
                _userManager.DeleteAsync(user).GetAwaiter().GetResult();
            }
            return RedirectToAction("Index");
        }
    }
}
