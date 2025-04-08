using Inyama_Yethu.Data;
using Inyama_Yethu.Models;
using Inyama_Yethu.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Inyama_Yethu.Areas.Employee.Controllers
{
    [Area("Employee")]
    [Authorize(Roles = "Employee,SeniorEmployee")]
    public class FeedRecordsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly TimeZoneInfo _southAfricaTimeZone;

        public FeedRecordsController(ApplicationDbContext context, TimeZoneInfo timeZoneInfo)
        {
            _context = context;
            _southAfricaTimeZone = timeZoneInfo;
        }

        // GET: Employee/FeedRecords
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
            var thirtyDaysAgo = today.AddDays(-30);
            
            // Get feed records for the last 30 days
            var feedRecords = await _context.Feedings
                .Include(f => f.Animal)
                .Where(f => f.FeedingDate >= thirtyDaysAgo)
                .OrderByDescending(f => f.FeedingDate)
                .ToListAsync();

            // Group feed records by feed type
            var feedByType = feedRecords
                .GroupBy(f => f.FeedType)
                .Select(g => new FeedTypeViewModel 
                { 
                    Type = g.Key, 
                    TotalAmount = g.Sum(f => f.Amount) 
                })
                .ToList();

            ViewData["Employee"] = employee;
            ViewData["FeedByType"] = feedByType;
            
            return View(feedRecords);
        }

        // GET: Employee/FeedRecords/Create
        public async Task<IActionResult> Create()
        {
            // Get the current employee based on the logged in user
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.Email == userEmail && e.IsActive);

            if (employee == null)
            {
                return RedirectToAction("AccessDenied", "Account", new { area = "" });
            }

            // Get animals for the dropdown
            var animals = await _context.Animals
                .Where(a => a.Status != AnimalStatus.Sold && a.Status != AnimalStatus.Deceased)
                .OrderBy(a => a.Type)
                .ThenBy(a => a.TagNumber)
                .ToListAsync();

            ViewData["AnimalId"] = new SelectList(animals, "Id", "TagNumber");
            ViewData["Employee"] = employee;
            
            // Set default date to today
            var feeding = new Feeding
            {
                FeedingDate = TimeZoneInfo.ConvertTime(DateTime.Now, _southAfricaTimeZone),
                RecordedById = employee.Id
            };
            
            return View(feeding);
        }

        // POST: Employee/FeedRecords/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AnimalId,FeedType,Amount,FeedingDate,Notes,CostPerKg")] Feeding feeding)
        {
            // Get the current employee based on the logged in user
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.Email == userEmail && e.IsActive);

            if (employee == null)
            {
                return RedirectToAction("AccessDenied", "Account", new { area = "" });
            }

            if (ModelState.IsValid)
            {
                feeding.RecordedById = employee.Id;
                feeding.RecordedDate = TimeZoneInfo.ConvertTime(DateTime.Now, _southAfricaTimeZone);
                
                _context.Add(feeding);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            
            // If we got this far, something failed, redisplay form
            var animals = await _context.Animals
                .Where(a => a.Status != AnimalStatus.Sold && a.Status != AnimalStatus.Deceased)
                .OrderBy(a => a.Type)
                .ThenBy(a => a.TagNumber)
                .ToListAsync();

            ViewData["AnimalId"] = new SelectList(animals, "Id", "TagNumber", feeding.AnimalId);
            ViewData["Employee"] = employee;
            
            return View(feeding);
        }

        // GET: Employee/FeedRecords/Details/5
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

            var feeding = await _context.Feedings
                .Include(f => f.Animal)
                .Include(f => f.RecordedBy)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (feeding == null)
            {
                return NotFound();
            }

            return View(feeding);
        }

        [Authorize(Policy = "CanManageFeed")]
        public IActionResult ManageInventory()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Policy = "CanManageFeed")]
        public async Task<IActionResult> UpdateInventory(FeedInventoryViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Implementation for updating feed inventory
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        [Authorize(Policy = "CanManageFeed")]
        public IActionResult CreateFeedSchedule()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Policy = "CanManageFeed")]
        public async Task<IActionResult> CreateFeedSchedule(FeedScheduleViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Implementation for creating feed schedule
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        [Authorize(Policy = "CanManageFeed")]
        public IActionResult AllocateBudget()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Policy = "CanManageFeed")]
        public async Task<IActionResult> AllocateBudget(FeedBudgetViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Implementation for allocating feed budget
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }
    }
} 