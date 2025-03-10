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
    public class PigletProcessingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly TimeZoneInfo _southAfricaTimeZone;

        public PigletProcessingController(ApplicationDbContext context, TimeZoneInfo timeZoneInfo)
        {
            _context = context;
            _southAfricaTimeZone = timeZoneInfo;
        }

        // GET: Employee/PigletProcessing
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
            var fourteenDaysAgo = today.AddDays(-14);

            // Get all piglet processing records
            var processingRecords = await _context.PigletProcessings
                .Include(p => p.Animal)
                .OrderByDescending(p => p.TailDockingDate)
                .ToListAsync();

            // Get recent farrowings (within last 14 days) to identify piglets needing processing
            var recentFarrowings = await _context.Matings
                .Include(m => m.Mother)
                .Include(m => m.Offspring)
                .Where(m => m.Status == MatingStatus.Farrowed && 
                           m.ActualFarrowingDate.HasValue && 
                           m.ActualFarrowingDate.Value >= fourteenDaysAgo)
                .ToListAsync();

            // Get piglets that need processing (born in last 14 days but not processed yet)
            var piglets = await _context.Animals
                .Where(a => a.Type == AnimalType.Piglet && 
                          a.BirthDate >= fourteenDaysAgo)
                .ToListAsync();

            // Get piglet processing tasks
            var processingTasks = await _context.TaskAssignments
                .Include(t => t.Animal)
                .Where(t => t.EmployeeId == employee.Id && 
                          (t.Category == "Piglet Processing" || t.Category == "Piglet Care") &&
                          t.Status != FarmTaskStatus.Completed)
                .ToListAsync();

            // Identify which piglets have been processed
            var processedPigletIds = processingRecords.Select(p => p.AnimalId).ToList();
            var unprocessedPiglets = piglets.Where(p => !processedPigletIds.Contains(p.Id)).ToList();

            // Pass data to view
            ViewData["Employee"] = employee;
            ViewData["Today"] = today;
            ViewData["ProcessingRecords"] = processingRecords;
            ViewData["RecentFarrowings"] = recentFarrowings;
            ViewData["UnprocessedPiglets"] = unprocessedPiglets;
            ViewData["ProcessingTasks"] = processingTasks;

            return View();
        }

        // GET: Employee/PigletProcessing/Details/5
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

            var processingRecord = await _context.PigletProcessings
                .Include(p => p.Animal)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (processingRecord == null)
            {
                return NotFound();
            }

            return View(processingRecord);
        }
    }
} 