using Inyama_Yethu.Data;
using Inyama_Yethu.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Inyama_Yethu.Areas.Employee.Controllers
{
    [Area("Employee")]
    [Authorize(Roles = "Employee")]
    public class MatingsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly TimeZoneInfo _southAfricaTimeZone;

        public MatingsController(ApplicationDbContext context, TimeZoneInfo timeZoneInfo)
        {
            _context = context;
            _southAfricaTimeZone = timeZoneInfo;
        }

        // GET: Employee/Matings
        public async Task<IActionResult> Index()
        {
            // Get the current employee based on the logged in user
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.Email == userEmail && e.IsActive);

            if (employee == null)
            {
                return RedirectToAction("AccessDenied", "Account", new { area = "" });
            }

            var today = TimeZoneInfo.ConvertTime(DateTime.Now, _southAfricaTimeZone).Date;
            
            // Calculate date thresholds for queries
            var fourteenDaysFromNow = today.AddDays(14);
            var fourteenDaysAgo = today.AddDays(-14);

            // Get all matings
            var matings = await _context.Matings
                .Include(m => m.Mother)
                .Include(m => m.Father)
                .OrderByDescending(m => m.MatingDate)
                .ToListAsync();

            // Categorize matings
            var scheduledMatings = matings
                .Where(m => m.Status == MatingStatus.Scheduled)
                .ToList();

            var pregnantSows = matings
                .Where(m => m.Status == MatingStatus.PregnancyConfirmed)
                .ToList();
            
            var upcomingFarrowings = matings
                .Where(m => m.Status == MatingStatus.PregnancyConfirmed && 
                           m.ExpectedFarrowingDate <= fourteenDaysFromNow &&
                           m.ExpectedFarrowingDate >= today)
                .ToList();

            var recentFarrowings = matings
                .Where(m => m.Status == MatingStatus.Farrowed && 
                           m.ActualFarrowingDate.HasValue && 
                           m.ActualFarrowingDate.Value >= fourteenDaysAgo)
                .ToList();

            // Pass data to view
            ViewData["Employee"] = employee;
            ViewData["Today"] = today;
            ViewData["ScheduledMatings"] = scheduledMatings;
            ViewData["PregnantSows"] = pregnantSows;
            ViewData["UpcomingFarrowings"] = upcomingFarrowings;
            ViewData["RecentFarrowings"] = recentFarrowings;

            return View();
        }

        // GET: Employee/Matings/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Get the current employee based on the logged in user
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.Email == userEmail && e.IsActive);

            if (employee == null)
            {
                return RedirectToAction("AccessDenied", "Account", new { area = "" });
            }

            var mating = await _context.Matings
                .Include(m => m.Mother)
                .Include(m => m.Father)
                .Include(m => m.Offspring)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (mating == null)
            {
                return NotFound();
            }

            return View(mating);
        }
    }
} 