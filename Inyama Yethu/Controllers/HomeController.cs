using System.Diagnostics;
using Inyama_Yethu.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Inyama_Yethu.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<IdentityUser> _userManager;

        public HomeController(
            ILogger<HomeController> logger,
            UserManager<IdentityUser> userManager)
        {
            _logger = logger;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            // If user is authenticated, redirect based on role
            if (User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                
                if (user != null)
                {
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
                        return RedirectToAction("Index", "Home", new { area = "Customer" });
                    }
                }
            }
            
            // If not authenticated or no specific role, show public home page
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
        
        [Authorize]
        public async Task<IActionResult> About()
        {
            // If user is authenticated, redirect based on role (for any authenticated action)
            if (User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                
                if (user != null)
                {
                    if (await _userManager.IsInRoleAsync(user, "Administrator"))
                    {
                        return RedirectToAction("About", "Home", new { area = "Admin" });
                    }
                    else if (await _userManager.IsInRoleAsync(user, "Employee"))
                    {
                        return RedirectToAction("About", "Home", new { area = "Employee" });
                    }
                    else if (await _userManager.IsInRoleAsync(user, "Customer"))
                    {
                        return RedirectToAction("About", "Home", new { area = "Customer" });
                    }
                }
            }
            
            ViewData["Message"] = "Inyama Yethu: Farm Management System";
            ViewData["Description"] = "A comprehensive system to manage pig farm operations, workforce, livestock breeding cycles, and business operations.";
            
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
