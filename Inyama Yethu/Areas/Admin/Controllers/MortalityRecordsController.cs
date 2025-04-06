using Inyama_Yethu.Data;
using Inyama_Yethu.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Inyama_Yethu.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Administrator")]
    public class MortalityRecordsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MortalityRecordsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/MortalityRecords
        public async Task<IActionResult> Index()
        {
            // Get deceased animals with their health records
            var deceasedAnimals = await _context.Animals
                .Where(a => a.Status == AnimalStatus.Deceased)
                .Include(a => a.HealthRecords.Where(hr => hr.Treatment == "Deceased"))
                .OrderByDescending(a => a.HealthRecords
                    .Where(hr => hr.Treatment == "Deceased")
                    .Select(hr => hr.RecordDate)
                    .FirstOrDefault())
                .ToListAsync();

            // Calculate total mortality
            var totalMortality = deceasedAnimals.Count;

            // Calculate mortality rate (deceased / total animals)
            var totalAnimals = await _context.Animals.CountAsync();
            var mortalityRate = totalAnimals > 0 ? (double)totalMortality / totalAnimals * 100 : 0;

            // Get monthly mortality counts
            var monthlyMortality = deceasedAnimals
                .GroupBy(a => a.HealthRecords
                    .Where(hr => hr.Treatment == "Deceased")
                    .Select(hr => new { hr.RecordDate.Year, hr.RecordDate.Month })
                    .FirstOrDefault())
                .Select(g => new
                {
                    YearMonth = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(g => g.YearMonth?.Year)
                .ThenByDescending(g => g.YearMonth?.Month)
                .Take(6)
                .ToList();

            // Get mortality by type/cause
            var mortalityByType = deceasedAnimals
                .GroupBy(a => a.HealthRecords
                    .Where(hr => hr.Treatment == "Deceased")
                    .Select(hr => hr.Description)
                    .FirstOrDefault())
                .Select(g => new
                {
                    Cause = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(g => g.Count)
                .ToList();

            ViewBag.TotalMortality = totalMortality;
            ViewBag.MortalityRate = mortalityRate;
            ViewBag.MonthlyMortality = monthlyMortality;
            ViewBag.MortalityByType = mortalityByType;

            return View(deceasedAnimals);
        }

        // GET: Admin/MortalityRecords/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var animal = await _context.Animals
                .Include(a => a.HealthRecords)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (animal == null)
            {
                return NotFound();
            }

            return View(animal);
        }

        // GET: Admin/MortalityRecords/Record
        public IActionResult Record()
        {
            var activeAnimals = _context.Animals
                .Where(a => a.Status == AnimalStatus.Active)
                .OrderBy(a => a.TagNumber)
                .ToList();

            ViewBag.Animals = new SelectList(activeAnimals, "Id", "TagNumber");
            return View();
        }

        // POST: Admin/MortalityRecords/Record
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Record(int AnimalId, DateTime DeathDate, string Cause, double? Weight, string Notes)
        {
            var animal = await _context.Animals.FindAsync(AnimalId);
            
            if (animal == null)
            {
                return NotFound();
            }

            // Update animal status to deceased
            animal.Status = AnimalStatus.Deceased;

            // Create a health record for the death
            var healthRecord = new HealthRecord
            {
                AnimalId = AnimalId,
                RecordDate = DeathDate,
                RecordType = HealthRecordType.Other,
                Treatment = "Deceased",
                Description = Cause,
                Notes = Notes
            };

            _context.Update(animal);
            _context.HealthRecords.Add(healthRecord);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/MortalityRecords/Report
        public async Task<IActionResult> Report(DateTime? startDate = null, DateTime? endDate = null)
        {
            // Default to last 30 days if no dates provided
            if (startDate == null)
                startDate = DateTime.Today.AddDays(-30);
            
            if (endDate == null)
                endDate = DateTime.Today;

            // Get health records for deceased animals in the date range
            var deathRecords = await _context.HealthRecords
                .Include(hr => hr.Animal)
                .Where(hr => hr.Treatment == "Deceased" && 
                       hr.RecordDate >= startDate && 
                       hr.RecordDate <= endDate)
                .OrderBy(hr => hr.RecordDate)
                .ToListAsync();

            // Calculate total mortality and rate
            var totalMortality = deathRecords.Count;
            var totalAnimals = await _context.Animals.CountAsync();
            var mortalityRate = totalAnimals > 0 ? (double)totalMortality / totalAnimals * 100 : 0;

            // Group by cause
            var mortalityByCause = deathRecords
                .GroupBy(hr => hr.Description)
                .Select(g => new
                {
                    Cause = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(g => g.Count)
                .ToList();

            // Primary cause of mortality
            var primaryCause = mortalityByCause.OrderByDescending(m => m.Count).FirstOrDefault()?.Cause ?? "None";

            // Group by age category
            var mortalityByAge = deathRecords
                .GroupBy(hr => GetAgeCategory(hr.Animal.BirthDate, hr.RecordDate))
                .Select(g => new
                {
                    AgeGroup = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(g => g.Count)
                .ToList();

            // Highest mortality age group
            var highestAgeGroup = mortalityByAge.OrderByDescending(m => m.Count).FirstOrDefault()?.AgeGroup ?? "None";

            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;
            ViewBag.TotalMortality = totalMortality;
            ViewBag.MortalityRate = mortalityRate;
            ViewBag.MortalityByCause = mortalityByCause;
            ViewBag.PrimaryCause = primaryCause;
            ViewBag.MortalityByAge = mortalityByAge;
            ViewBag.HighestAgeGroup = highestAgeGroup;

            return View(deathRecords);
        }

        private string GetAgeCategory(DateTime? birthDate, DateTime deathDate)
        {
            if (!birthDate.HasValue)
                return "Unknown";

            var age = (deathDate - birthDate.Value).TotalDays;

            if (age < 7)
                return "Neonatal (0-7 days)";
            else if (age < 28)
                return "Suckling (7-28 days)";
            else if (age < 70)
                return "Weaner (28-70 days)";
            else if (age < 150)
                return "Grower (70-150 days)";
            else if (age < 730)
                return "Finisher/Gilt (150-730 days)";
            else
                return "Adult (> 730 days)";
        }
    }

    public class MortalityRecordViewModel
    {
        public int AnimalId { get; set; }
        public DateTime DeathDate { get; set; }
        public string Cause { get; set; }
        public double? Weight { get; set; }
        public string Notes { get; set; }
    }
} 