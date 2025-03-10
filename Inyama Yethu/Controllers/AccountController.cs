using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Inyama_Yethu.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public AccountController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [Authorize]
        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            // Redirect based on role
            if (await _userManager.IsInRoleAsync(user, "Administrator"))
            {
                return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
            }
            else if (await _userManager.IsInRoleAsync(user, "Employee"))
            {
                return RedirectToAction("Index", "Dashboard", new { area = "Employee" });
            }
            else if (await _userManager.IsInRoleAsync(user, "Customer"))
            {
                return RedirectToAction("Dashboard", "Account", new { area = "Customer" });
            }
            else
            {
                // If no specific role, redirect to home page
                return RedirectToAction("Index", "Home");
            }
        }

        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            // Redirect based on role
            if (await _userManager.IsInRoleAsync(user, "Administrator"))
            {
                return RedirectToAction("Profile", "Account", new { area = "Admin" });
            }
            else if (await _userManager.IsInRoleAsync(user, "Employee"))
            {
                return RedirectToAction("Profile", "Account", new { area = "Employee" });
            }
            else if (await _userManager.IsInRoleAsync(user, "Customer"))
            {
                return RedirectToAction("Profile", "Account", new { area = "Customer" });
            }
            else
            {
                // If no specific role, redirect to manage page in Identity area
                return RedirectToAction("Manage/Index", "Account", new { area = "Identity" });
            }
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
} 