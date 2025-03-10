using Inyama_Yethu.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Inyama_Yethu.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Get featured products for the homepage
            var featuredProducts = await _context.Products
                .Where(p => p.IsAvailable && p.StockQuantity > 0)
                .OrderByDescending(p => p.CreatedDate)
                .Take(6)
                .ToListAsync();

            return View(featuredProducts);
        }

        public IActionResult About()
        {
            ViewData["Title"] = "About Inyama Yethu";
            ViewData["Message"] = "Quality meats for township shisanyamas since 2015.";
            
            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Title"] = "Contact Us";
            ViewData["Message"] = "Have questions or feedback? We'd love to hear from you.";
            
            ViewData["Address"] = "123 Farm Road, Cape Town, Western Cape";
            ViewData["Phone"] = "(012) 345-6789";
            ViewData["Email"] = "info@inyamayethu.co.za";
            
            return View();
        }

        public IActionResult PrivacyPolicy()
        {
            return View();
        }

        public IActionResult Terms()
        {
            return View();
        }

        [Authorize(Roles = "Customer")]
        public IActionResult Dashboard()
        {
            return RedirectToAction("Dashboard", "Account");
        }
    }
} 