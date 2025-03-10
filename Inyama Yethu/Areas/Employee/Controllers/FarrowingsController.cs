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
    public class FarrowingsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly TimeZoneInfo _southAfricaTimeZone;

        public FarrowingsController(ApplicationDbContext context, TimeZoneInfo timeZoneInfo)
        {
            _context = context;
            _southAfricaTimeZone = timeZoneInfo;
        }

        // GET: Employee/Farrowings
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
            var thirtyDaysFromNow = today.AddDays(30);
            var sevenDaysFromNow = today.AddDays(7);
            var thirtyDaysAgo = today.AddDays(-30);

            // Get matings that are in the farrowing stage
            var matings = await _context.Matings
                .Include(m => m.Mother)
                .Include(m => m.Father)
                .Where(m => m.Status == MatingStatus.PregnancyConfirmed || m.Status == MatingStatus.Farrowed)
                .OrderBy(m => m.ExpectedFarrowingDate)
                .ToListAsync();

            // Upcoming farrowings (within next 30 days)
            var upcomingFarrowings = matings
                .Where(m => m.Status == MatingStatus.PregnancyConfirmed && 
                           m.ExpectedFarrowingDate <= thirtyDaysFromNow &&
                           m.ExpectedFarrowingDate >= today)
                .ToList();

            // Farrowings due this week
            var farrowingsDueThisWeek = matings
                .Where(m => m.Status == MatingStatus.PregnancyConfirmed && 
                           m.ExpectedFarrowingDate <= sevenDaysFromNow &&
                           m.ExpectedFarrowingDate >= today)
                .ToList();

            // Recent farrowings (within last 30 days)
            var recentFarrowings = matings
                .Where(m => m.Status == MatingStatus.Farrowed && 
                           m.ActualFarrowingDate.HasValue && 
                           m.ActualFarrowingDate.Value >= thirtyDaysAgo)
                .ToList();

            // Pass data to view
            ViewData["Employee"] = employee;
            ViewData["Today"] = today;
            ViewData["UpcomingFarrowings"] = upcomingFarrowings;
            ViewData["FarrowingsDueThisWeek"] = farrowingsDueThisWeek;
            ViewData["RecentFarrowings"] = recentFarrowings;
            
            // Get related tasks for farrowings
            var farrowingTasks = await _context.TaskAssignments
                .Where(t => t.EmployeeId == employee.Id && 
                          (t.Category == "Farrowing" || t.Category == "Piglet Care") &&
                          t.Status != FarmTaskStatus.Completed)
                .ToListAsync();
                
            ViewData["FarrowingTasks"] = farrowingTasks;

            return View();
        }

        // GET: Employee/Farrowings/Details/5
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