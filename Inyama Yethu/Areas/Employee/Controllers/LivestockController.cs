using Inyama_Yethu.Data;
using Inyama_Yethu.Models;
using Inyama_Yethu.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
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
        private readonly IActivityLogService _activityLogService;

        public LivestockController(
            ApplicationDbContext context, 
            TimeZoneInfo timeZoneInfo,
            IActivityLogService activityLogService)
        {
            _context = context;
            _southAfricaTimeZone = timeZoneInfo;
            _activityLogService = activityLogService;
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

            // Log activity for viewing animal details
            await _activityLogService.LogActivityAsync(
                "Animal",
                animal.Id,
                ActivityType.Read,
                $"Viewed details of animal {animal.TagNumber}"
            );

            return View(animal);
        }

        // GET: Employee/Livestock/Create
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

            // Get potential parents for dropdown lists
            var potentialMothers = await _context.Animals
                .Where(a => a.Gender == Gender.Female && a.Type == AnimalType.Sow)
                .OrderBy(a => a.TagNumber)
                .ToListAsync();

            var potentialFathers = await _context.Animals
                .Where(a => a.Gender == Gender.Male && a.Type == AnimalType.Boar)
                .OrderBy(a => a.TagNumber)
                .ToListAsync();

            ViewData["MotherAnimalId"] = new SelectList(potentialMothers, "Id", "TagNumber");
            ViewData["FatherAnimalId"] = new SelectList(potentialFathers, "Id", "TagNumber");
            ViewData["Employee"] = employee;

            return View();
        }

        // POST: Employee/Livestock/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TagNumber,Type,Gender,BirthDate,Status,Weight,Notes,MotherAnimalId,FatherAnimalId")] Animal animal)
        {
            // Get the current employee based on the logged in user
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.Email == userEmail && e.IsActive);

            if (employee == null)
            {
                return RedirectToAction("AccessDenied", "Account", new { area = "" });
            }

            // Log model state errors for debugging
            if (!ModelState.IsValid)
            {
                _activityLogService.LogActivityAsync(
                    "Animal", 
                    0, 
                    ActivityType.Create, 
                    $"Create animal form validation failed: {string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage))}"
                ).Wait();
                
                foreach (var state in ModelState)
                {
                    foreach (var error in state.Value.Errors)
                    {
                        // Add to TempData for display to user
                        TempData["ErrorMessage"] = $"Error in {state.Key}: {error.ErrorMessage}";
                    }
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Set creation timestamp
                    animal.CreatedAt = TimeZoneInfo.ConvertTime(DateTime.Now, _southAfricaTimeZone);
                    animal.UpdatedAt = animal.CreatedAt;

                    _context.Add(animal);
                    await _context.SaveChangesAsync();

                    // Log the activity
                    await _activityLogService.LogActivityAsync(
                        "Animal",
                        animal.Id,
                        ActivityType.Create,
                        $"Created new animal with tag number {animal.TagNumber}",
                        null,
                        JsonConvert.SerializeObject(animal)
                    );

                    TempData["SuccessMessage"] = $"Animal with tag {animal.TagNumber} was successfully created.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    // Log the exception
                    await _activityLogService.LogActivityAsync(
                        "Animal",
                        0,
                        ActivityType.Create,
                        $"Error creating animal: {ex.Message}",
                        null,
                        JsonConvert.SerializeObject(animal)
                    );
                    
                    ModelState.AddModelError("", $"Error saving: {ex.Message}");
                    TempData["ErrorMessage"] = $"Error creating animal: {ex.Message}";
                }
            }

            // If we got this far, something failed, redisplay form
            var potentialMothers = await _context.Animals
                .Where(a => a.Gender == Gender.Female && a.Type == AnimalType.Sow)
                .OrderBy(a => a.TagNumber)
                .ToListAsync();

            var potentialFathers = await _context.Animals
                .Where(a => a.Gender == Gender.Male && a.Type == AnimalType.Boar)
                .OrderBy(a => a.TagNumber)
                .ToListAsync();

            ViewData["MotherAnimalId"] = new SelectList(potentialMothers, "Id", "TagNumber", animal.MotherAnimalId);
            ViewData["FatherAnimalId"] = new SelectList(potentialFathers, "Id", "TagNumber", animal.FatherAnimalId);
            ViewData["Employee"] = employee;

            return View(animal);
        }

        // GET: Employee/Livestock/Edit/5
        public async Task<IActionResult> Edit(int? id)
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

            var animal = await _context.Animals.FindAsync(id);
            if (animal == null)
            {
                return NotFound();
            }

            // Get potential parents for dropdown lists
            var potentialMothers = await _context.Animals
                .Where(a => a.Gender == Gender.Female && a.Type == AnimalType.Sow && a.Id != id)
                .OrderBy(a => a.TagNumber)
                .ToListAsync();

            var potentialFathers = await _context.Animals
                .Where(a => a.Gender == Gender.Male && a.Type == AnimalType.Boar && a.Id != id)
                .OrderBy(a => a.TagNumber)
                .ToListAsync();

            ViewData["MotherAnimalId"] = new SelectList(potentialMothers, "Id", "TagNumber", animal.MotherAnimalId);
            ViewData["FatherAnimalId"] = new SelectList(potentialFathers, "Id", "TagNumber", animal.FatherAnimalId);
            ViewData["Employee"] = employee;

            return View(animal);
        }

        // POST: Employee/Livestock/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TagNumber,Type,Gender,BirthDate,Status,Weight,Notes,MotherAnimalId,FatherAnimalId")] Animal animal)
        {
            if (id != animal.Id)
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

            if (ModelState.IsValid)
            {
                try
                {
                    // Get original animal for change tracking
                    var originalAnimal = await _context.Animals.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id);
                    
                    // Update the UpdatedAt timestamp
                    animal.UpdatedAt = TimeZoneInfo.ConvertTime(DateTime.Now, _southAfricaTimeZone);
                    // Preserve the original CreatedAt date
                    animal.CreatedAt = originalAnimal.CreatedAt;

                    _context.Update(animal);
                    await _context.SaveChangesAsync();

                    // Log the activity with what changed
                    await _activityLogService.LogActivityAsync(
                        "Animal",
                        animal.Id,
                        ActivityType.Update,
                        $"Updated animal with tag number {animal.TagNumber}",
                        JsonConvert.SerializeObject(originalAnimal),
                        JsonConvert.SerializeObject(animal)
                    );
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AnimalExists(animal.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            // If we got this far, something failed, redisplay form
            var potentialMothers = await _context.Animals
                .Where(a => a.Gender == Gender.Female && a.Type == AnimalType.Sow && a.Id != id)
                .OrderBy(a => a.TagNumber)
                .ToListAsync();

            var potentialFathers = await _context.Animals
                .Where(a => a.Gender == Gender.Male && a.Type == AnimalType.Boar && a.Id != id)
                .OrderBy(a => a.TagNumber)
                .ToListAsync();

            ViewData["MotherAnimalId"] = new SelectList(potentialMothers, "Id", "TagNumber", animal.MotherAnimalId);
            ViewData["FatherAnimalId"] = new SelectList(potentialFathers, "Id", "TagNumber", animal.FatherAnimalId);
            ViewData["Employee"] = employee;

            return View(animal);
        }

        // GET: Employee/Livestock/Delete/5
        public async Task<IActionResult> Delete(int? id)
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
                .Include(a => a.Mother)
                .Include(a => a.Father)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (animal == null)
            {
                return NotFound();
            }

            return View(animal);
        }

        // POST: Employee/Livestock/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Get the current employee based on the logged in user
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.Email == userEmail && e.IsActive);

            if (employee == null)
            {
                return RedirectToAction("AccessDenied", "Account", new { area = "" });
            }

            var animal = await _context.Animals.FindAsync(id);
            if (animal == null)
            {
                return NotFound();
            }

            // Save animal info before deletion for logging
            var animalInfo = JsonConvert.SerializeObject(animal);
            var tagNumber = animal.TagNumber;

            _context.Animals.Remove(animal);
            await _context.SaveChangesAsync();

            // Log the deletion
            await _activityLogService.LogActivityAsync(
                "Animal",
                id,
                ActivityType.Delete,
                $"Deleted animal with tag number {tagNumber}",
                animalInfo,
                null
            );

            return RedirectToAction(nameof(Index));
        }

        private bool AnimalExists(int id)
        {
            return _context.Animals.Any(e => e.Id == id);
        }

        // Special route to catch /admin/livestock/create URLs
        [AllowAnonymous]
        [HttpGet("/admin/livestock/create")]
        public IActionResult AdminLivestockCreate()
        {
            // Check if user is authenticated
            if (User.Identity.IsAuthenticated)
            {
                // If Admin, redirect to Admin Animals Create
                if (User.IsInRole("Administrator"))
                {
                    return RedirectToAction("Create", "Animals", new { area = "Admin" });
                }
                // If Employee, redirect to Employee Livestock Create
                else if (User.IsInRole("Employee"))
                {
                    return RedirectToAction("Create", "Livestock", new { area = "Employee" });
                }
            }
            
            // If not authenticated or other role, redirect to login with return URL
            return RedirectToAction("Login", "Account", new { 
                area = "", 
                returnUrl = "/Employee/Livestock/Create" 
            });
        }
    }
} 