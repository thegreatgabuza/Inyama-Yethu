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

        // GET: Employee/Matings/SowServiceCard/{id}
        public async Task<IActionResult> SowServiceCard(int? id)
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

            // Get the animal (sow) by id
            var animal = await _context.Animals
                .FirstOrDefaultAsync(a => a.Id == id && a.Gender == Gender.Female);

            if (animal == null)
            {
                return NotFound();
            }

            // Get all matings for this sow
            var matings = await _context.Matings
                .Include(m => m.Father)
                .Where(m => m.MotherAnimalId == id)
                .OrderByDescending(m => m.MatingDate)
                .ToListAsync();

            // Get all births for this sow
            var births = await _context.Births
                .Where(b => b.MotherAnimalId == id)
                .OrderByDescending(b => b.BirthDate)
                .ToListAsync();

            ViewData["Animal"] = animal;
            ViewData["Matings"] = matings;
            ViewData["Births"] = births;
            ViewData["Employee"] = employee;

            return View();
        }

        // GET: Employee/Matings/RecordService/{id}
        public async Task<IActionResult> RecordService(int? id)
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

            // Get the animal (sow) by id
            var animal = await _context.Animals
                .FirstOrDefaultAsync(a => a.Id == id && a.Gender == Gender.Female && a.Status != AnimalStatus.Deceased);

            if (animal == null)
            {
                return NotFound();
            }

            // Get available boars for service
            var boars = await _context.Animals
                .Where(a => a.Gender == Gender.Male && a.Type == AnimalType.Boar && a.Status == AnimalStatus.Active)
                .OrderBy(a => a.TagNumber)
                .ToListAsync();

            ViewData["FatherAnimalId"] = new SelectList(boars, "Id", "TagNumber");
            ViewData["Animal"] = animal;
            ViewData["Employee"] = employee;

            var mating = new Mating
            {
                MotherAnimalId = animal.Id,
                MatingDate = TimeZoneInfo.ConvertTime(DateTime.Now, _southAfricaTimeZone),
                Status = MatingStatus.Scheduled
            };

            return View(mating);
        }

        // POST: Employee/Matings/RecordService
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RecordService([Bind("MotherAnimalId,FatherAnimalId,MatingDate,Notes")] Mating mating)
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
                // Set status and recorded by information
                mating.Status = MatingStatus.Scheduled;
                mating.RecordedById = employee.Id;
                mating.RecordedDate = TimeZoneInfo.ConvertTime(DateTime.Now, _southAfricaTimeZone);
                
                // Calculate expected farrowing date (approximately 114 days from mating)
                mating.ExpectedFarrowingDate = mating.MatingDate.AddDays(114);
                
                // Update sow status
                var sow = await _context.Animals.FindAsync(mating.MotherAnimalId);
                if (sow != null)
                {
                    sow.Status = AnimalStatus.Mating;
                    sow.UpdatedAt = DateTime.Now;
                    _context.Update(sow);
                }
                
                _context.Add(mating);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Service record successfully added.";
                return RedirectToAction("SowServiceCard", new { id = mating.MotherAnimalId });
            }

            // If we got this far, something failed, redisplay form
            var animal = await _context.Animals
                .FirstOrDefaultAsync(a => a.Id == mating.MotherAnimalId);

            var boars = await _context.Animals
                .Where(a => a.Gender == Gender.Male && a.Type == AnimalType.Boar && a.Status == AnimalStatus.Active)
                .OrderBy(a => a.TagNumber)
                .ToListAsync();

            ViewData["FatherAnimalId"] = new SelectList(boars, "Id", "TagNumber", mating.FatherAnimalId);
            ViewData["Animal"] = animal;
            ViewData["Employee"] = employee;

            return View(mating);
        }
    }
} 