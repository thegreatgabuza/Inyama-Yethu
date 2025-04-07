using Inyama_Yethu.Data;
using Inyama_Yethu.Models;
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
    [Authorize(Roles = "Employee")]
    public class HealthRecordsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly TimeZoneInfo _southAfricaTimeZone;

        public HealthRecordsController(ApplicationDbContext context, TimeZoneInfo timeZoneInfo)
        {
            _context = context;
            _southAfricaTimeZone = timeZoneInfo;
        }

        // GET: Employee/HealthRecords
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

            // Get health records for the last 30 days
            var healthRecords = await _context.HealthRecords
                .Include(h => h.Animal)
                .Where(h => h.RecordDate >= thirtyDaysAgo)
                .OrderByDescending(h => h.RecordDate)
                .ToListAsync();

            // Group health records by treatment type
            var healthByType = healthRecords
                .GroupBy(h => h.TreatmentType)
                .Select(g => new HealthTypeViewModel 
                { 
                    Type = g.Key, 
                    Count = g.Count() 
                })
                .ToList();

            ViewData["Employee"] = employee;
            ViewData["HealthByType"] = healthByType;

            return View(healthRecords);
        }

        // GET: Employee/HealthRecords/Create
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
            var healthRecord = new HealthRecord
            {
                RecordDate = TimeZoneInfo.ConvertTime(DateTime.Now, _southAfricaTimeZone),
                PerformedById = employee.Id
            };

            return View(healthRecord);
        }

        // POST: Employee/HealthRecords/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AnimalId,TreatmentType,Description,RecordDate,Notes,Cost,Medication,Dosage,TreatmentOutcome")] HealthRecord healthRecord)
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
                healthRecord.PerformedById = employee.Id;

                _context.Add(healthRecord);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // If we got this far, something failed, redisplay form
            var animals = await _context.Animals
                .Where(a => a.Status != AnimalStatus.Sold && a.Status != AnimalStatus.Deceased)
                .OrderBy(a => a.Type)
                .ThenBy(a => a.TagNumber)
                .ToListAsync();

            ViewData["AnimalId"] = new SelectList(animals, "Id", "TagNumber", healthRecord.AnimalId);
            ViewData["Employee"] = employee;

            return View(healthRecord);
        }

        // GET: Employee/HealthRecords/Details/5
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

            var healthRecord = await _context.HealthRecords
                .Include(h => h.Animal)
                .Include(h => h.PerformedBy)
                .FirstOrDefaultAsync(h => h.Id == id);

            if (healthRecord == null)
            {
                return NotFound();
            }

            return View(healthRecord);
        }

        // GET: Employee/HealthRecords/MortalityRecord
        public async Task<IActionResult> MortalityRecord()
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
            
            var viewModel = new MortalityRecordViewModel
            {
                RecordDate = TimeZoneInfo.ConvertTime(DateTime.Now, _southAfricaTimeZone)
            };

            return View(viewModel);
        }

        // POST: Employee/HealthRecords/MortalityRecord
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MortalityRecord(MortalityRecordViewModel viewModel)
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
                // Create a health record with mortality information
                var healthRecord = new HealthRecord
                {
                    AnimalId = viewModel.AnimalId,
                    TreatmentType = "Mortality",
                    Description = "Animal death recorded",
                    RecordDate = viewModel.RecordDate,
                    Notes = viewModel.CauseOfDeath,
                    PerformedById = employee.Id,
                    TreatmentOutcome = "Deceased"
                };

                // Update the animal status
                var animal = await _context.Animals.FindAsync(viewModel.AnimalId);
                if (animal != null)
                {
                    animal.Status = AnimalStatus.Deceased;
                    animal.UpdatedAt = DateTime.Now;
                    
                    _context.Add(healthRecord);
                    _context.Update(animal);
                    await _context.SaveChangesAsync();
                    
                    TempData["SuccessMessage"] = $"Mortality record for animal {animal.TagNumber} successfully recorded.";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ModelState.AddModelError("AnimalId", "The selected animal could not be found.");
                }
            }

            // If we got this far, something failed, redisplay form
            var animals = await _context.Animals
                .Where(a => a.Status != AnimalStatus.Sold && a.Status != AnimalStatus.Deceased)
                .OrderBy(a => a.Type)
                .ThenBy(a => a.TagNumber)
                .ToListAsync();

            ViewData["AnimalId"] = new SelectList(animals, "Id", "TagNumber", viewModel.AnimalId);
            ViewData["Employee"] = employee;

            return View(viewModel);
        }
    }
} 