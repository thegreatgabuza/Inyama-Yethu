using Inyama_Yethu.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Inyama_Yethu.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Administrator")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly TimeZoneInfo _southAfricaTimeZone;

        public DashboardController(ApplicationDbContext context, TimeZoneInfo timeZoneInfo)
        {
            _context = context;
            _southAfricaTimeZone = timeZoneInfo;
        }

        public async Task<IActionResult> Index()
        {
            var today = TimeZoneInfo.ConvertTime(DateTime.Now, _southAfricaTimeZone).Date;

            // Get employees and their status
            var employees = await _context.Employees
                .Select(e => new
                {
                    Employee = e,
                    CheckedIn = e.Attendances.Any(a => a.CheckInTime.Date == today && a.CheckOutTime == null)
                })
                .ToListAsync();

            // Get pending tasks
            var pendingTasks = await _context.TaskAssignments
                .Include(t => t.Employee)
                .Include(t => t.Animal)
                .Where(t => t.Status == Models.FarmTaskStatus.Pending)
                .OrderBy(t => t.DueDate)
                .Take(10)
                .ToListAsync();

            // Get livestock statistics
            var livestockStats = await _context.Animals
                .Where(a => a.Status != Models.AnimalStatus.Deceased && a.Status != Models.AnimalStatus.Sold)
                .GroupBy(a => a.Type)
                .Select(g => new { Type = g.Key, Count = g.Count() })
                .ToListAsync();

            // Get upcoming matings
            var upcomingMatings = await _context.Matings
                .Include(m => m.Mother)
                .Include(m => m.Father)
                .Where(m => m.Status == Models.MatingStatus.Scheduled && m.MatingDate >= today)
                .OrderBy(m => m.MatingDate)
                .Take(5)
                .ToListAsync();

            // Get upcoming farrowings
            var upcomingFarrowings = await _context.Matings
                .Include(m => m.Mother)
                .Where(m => m.Status == Models.MatingStatus.PregnancyConfirmed && m.ExpectedFarrowingDate >= today)
                .OrderBy(m => m.ExpectedFarrowingDate)
                .Take(5)
                .ToListAsync();

            // Get recent orders
            var recentOrders = await _context.Orders
                .Include(o => o.Customer)
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .ToListAsync();

            // Get recent feedback
            var recentFeedback = await _context.Feedbacks
                .Include(f => f.Customer)
                .Where(f => !f.HasBeenAddressed)
                .OrderByDescending(f => f.FeedbackDate)
                .Take(5)
                .ToListAsync();

            // Pass data to the view
            ViewData["Today"] = today;
            ViewData["Employees"] = employees;
            ViewData["PendingTasks"] = pendingTasks;
            ViewData["LivestockStats"] = livestockStats;
            ViewData["UpcomingMatings"] = upcomingMatings;
            ViewData["UpcomingFarrowings"] = upcomingFarrowings;
            ViewData["RecentOrders"] = recentOrders;
            ViewData["RecentFeedback"] = recentFeedback;

            return View();
        }

        public IActionResult Reports()
        {
            return View();
        }

        public IActionResult Settings()
        {
            return View();
        }
    }
} 