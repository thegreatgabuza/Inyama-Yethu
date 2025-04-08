using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Inyama_Yethu.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Inyama_Yethu.Areas.SeniorEmployee.Controllers
{
    [Area("SeniorEmployee")]
    [Authorize(Roles = "SeniorEmployee")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Get counts for dashboard
            ViewBag.TotalAnimals = await _context.Animals.CountAsync();
            ViewBag.ActiveTasks = await _context.TaskAssignments.Where(t => !t.IsCompleted).CountAsync();
            ViewBag.PendingHealthChecks = await _context.HealthRecords.Where(h => h.FollowUpDate != null && h.FollowUpDate > DateTime.Now && !h.FollowUpCompleted).CountAsync();
            ViewBag.LowFeedAlerts = await _context.FeedInventory.Where(f => f.CurrentStock < f.MinimumStockLevel).CountAsync();

            return View();
        }
    }
} 