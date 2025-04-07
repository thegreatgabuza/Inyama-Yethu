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
    public class LivestockController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly TimeZoneInfo _southAfricaTimeZone;

        public LivestockController(ApplicationDbContext context, TimeZoneInfo timeZoneInfo)
        {
            _context = context;
            _southAfricaTimeZone = timeZoneInfo;
        }

        // GET: Employee/Livestock
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

            // Get all animals
            var animals = await _context.Animals
                .OrderBy(a => a.Type)
                .ThenBy(a => a.TagNumber)
                .ToListAsync();

            // Get recent health records
            var recentHealthRecords = await _context.HealthRecords
                .Include(h => h.Animal)
                .Where(h => h.RecordDate >= thirtyDaysAgo)
                .OrderByDescending(h => h.RecordDate)
                .ToListAsync();

            // Get recent weight records
            var recentWeightRecords = await _context.WeightRecords
                .Include(w => w.Animal)
                .Where(w => w.RecordDate >= thirtyDaysAgo)
                .OrderByDescending(w => w.RecordDate)
                .ToListAsync();

            // Get livestock tasks
            var livestockTasks = await _context.TaskAssignments
                .Include(t => t.Animal)
                .Where(t => t.EmployeeId == employee.Id && 
                          (t.Category.Name == "Livestock" || t.Category.Name == "Health" || t.Category.Name == "Feeding") &&
                          t.Status != FarmTaskStatus.Completed)
                .ToListAsync();

            // Group animals by type
            var animalsByType = animals
                .GroupBy(a => a.Type)
                .Select(g => new { Type = g.Key, Count = g.Count() })
                .ToList();

            // Pass data to view
            ViewData["Employee"] = employee;
            ViewData["Today"] = today;
            ViewData["Animals"] = animals;
            ViewData["AnimalsByType"] = animalsByType;
            ViewData["RecentHealthRecords"] = recentHealthRecords;
            ViewData["RecentWeightRecords"] = recentWeightRecords;
            ViewData["LivestockTasks"] = livestockTasks;

            return View();
        }

        // GET: Employee/Livestock/Details/5
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

            var animal = await _context.Animals
                .FirstOrDefaultAsync(a => a.Id == id);

            if (animal == null)
            {
                return NotFound();
            }

            // Get health records for this animal
            var healthRecords = await _context.HealthRecords
                .Where(h => h.AnimalId == id)
                .OrderByDescending(h => h.RecordDate)
                .ToListAsync();

            // Get weight records for this animal
            var weightRecords = await _context.WeightRecords
                .Where(w => w.AnimalId == id)
                .OrderByDescending(w => w.RecordDate)
                .ToListAsync();

            // Get tasks for this animal
            var animalTasks = await _context.TaskAssignments
                .Where(t => t.AnimalId == id && t.Status != FarmTaskStatus.Completed)
                .OrderBy(t => t.DueDate)
                .ToListAsync();

            ViewData["HealthRecords"] = healthRecords;
            ViewData["WeightRecords"] = weightRecords;
            ViewData["AnimalTasks"] = animalTasks;

            return View(animal);
        }
    }
} 