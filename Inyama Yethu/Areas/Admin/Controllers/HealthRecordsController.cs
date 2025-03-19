using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Inyama_Yethu.Data;
using Inyama_Yethu.Models;
using Microsoft.AspNetCore.Authorization;
using System.Data;

namespace Inyama_Yethu.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Administrator")]
    public class HealthRecordsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly TimeZoneInfo _southAfricaTimeZone;

        public HealthRecordsController(ApplicationDbContext context, TimeZoneInfo timeZoneInfo)
        {
            _context = context;
            _southAfricaTimeZone = timeZoneInfo;
        }

        // GET: Admin/HealthRecords
        public async Task<IActionResult> Index()
        {
            var today = TimeZoneInfo.ConvertTime(DateTime.Now, _southAfricaTimeZone).Date;
            
            // Get all health records
            var healthRecords = await _context.HealthRecords
                .Include(h => h.Animal)
                .OrderByDescending(h => h.RecordDate)
                .ToListAsync();

            // Get animals due for vaccination
            var animalsNeedingVaccination = await GetAnimalsNeedingVaccination();
            ViewBag.AnimalsNeedingVaccination = animalsNeedingVaccination;

            return View(healthRecords);
        }

        // GET: Admin/HealthRecords/VaccinationSchedule
        public async Task<IActionResult> VaccinationSchedule()
        {
            var animals = await _context.Animals
                .Include(a => a.HealthRecords.Where(hr => hr.RecordType == HealthRecordType.Vaccination))
                .Where(a => a.Status != AnimalStatus.Deceased && a.Status != AnimalStatus.Sold)
                .ToListAsync();

            var schedule = new List<VaccinationScheduleViewModel>();
            var today = TimeZoneInfo.ConvertTime(DateTime.Now, _southAfricaTimeZone).Date;

            foreach (var animal in animals)
            {
                var lastVaccination = animal.HealthRecords
                    .OrderByDescending(hr => hr.RecordDate)
                    .FirstOrDefault();

                var nextVaccinationDue = lastVaccination != null
                    ? lastVaccination.RecordDate.AddMonths(1)
                    : today;

                schedule.Add(new VaccinationScheduleViewModel
                {
                    Animal = animal,
                    LastVaccinationDate = lastVaccination?.RecordDate,
                    NextVaccinationDue = nextVaccinationDue,
                    IsOverdue = nextVaccinationDue < today
                });
            }

            return View(schedule);
        }

        // POST: Admin/HealthRecords/ScheduleVaccinations
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ScheduleVaccinations(DateTime vaccinationDate)
        {
            var animalsNeedingVaccination = await GetAnimalsNeedingVaccination();

            foreach (var animal in animalsNeedingVaccination)
            {
                var healthRecord = new HealthRecord
                {
                    AnimalId = animal.Id,
                    RecordDate = vaccinationDate,
                    RecordType = HealthRecordType.Vaccination,
                    Treatment = "Monthly Routine Vaccination",
                    Description = "Scheduled monthly vaccination",
                    FollowUpDate = vaccinationDate.AddMonths(1),
                    FollowUpCompleted = false
                };

                _context.Add(healthRecord);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(VaccinationSchedule));
        }

        private async Task<List<Animal>> GetAnimalsNeedingVaccination()
        {
            var today = TimeZoneInfo.ConvertTime(DateTime.Now, _southAfricaTimeZone).Date;

            // Get all active animals
            var animals = await _context.Animals
                .Include(a => a.HealthRecords.Where(hr => hr.RecordType == HealthRecordType.Vaccination))
                .Where(a => a.Status != AnimalStatus.Deceased && a.Status != AnimalStatus.Sold)
                .ToListAsync();

            return animals.Where(animal =>
            {
                var lastVaccination = animal.HealthRecords
                    .OrderByDescending(hr => hr.RecordDate)
                    .FirstOrDefault();

                // Animal needs vaccination if:
                // 1. Never vaccinated, or
                // 2. Last vaccination was more than a month ago
                return lastVaccination == null || 
                       lastVaccination.RecordDate.AddMonths(1) <= today;
            }).ToList();
        }

        // GET: Admin/HealthRecords/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var healthRecord = await _context.HealthRecords
                .Include(h => h.Animal)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (healthRecord == null)
            {
                return NotFound();
            }

            return View(healthRecord);
        }

        // GET: Admin/HealthRecords/Create
        public IActionResult Create()
        {
            ViewBag.Animals = _context.Animals
                .Select(a => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = a.Id.ToString(),
                    Text = $"{a.TagNumber} - {a.Type}"
                })
                .ToList();

            return View();
        }

        // POST: Admin/HealthRecords/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AnimalId,RecordDate,RecordType,Treatment,AdministeredBy,Cost,FollowUpDate,FollowUpCompleted,Description,Notes")] HealthRecord healthRecord)
        {
            if (!ModelState.IsValid)
            {
                _context.Add(healthRecord);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Animals = _context.Animals
                .Select(a => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = a.Id.ToString(),
                    Text = $"{a.TagNumber} - {a.Type}"
                })
                .ToList();

            return View(healthRecord);
        }

        // GET: Admin/HealthRecords/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var healthRecord = await _context.HealthRecords.FindAsync(id);
            if (healthRecord == null)
            {
                return NotFound();
            }

            ViewBag.Animals = _context.Animals
                .Select(a => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = a.Id.ToString(),
                    Text = $"{a.TagNumber} - {a.Type}"
                })
                .ToList();

            return View(healthRecord);
        }

        // POST: Admin/HealthRecords/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,AnimalId,RecordDate,RecordType,Treatment,AdministeredBy,Cost,FollowUpDate,FollowUpCompleted,Description,Notes")] HealthRecord healthRecord)
        {
            if (id != healthRecord.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(healthRecord);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!HealthRecordExists(healthRecord.Id))
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

            ViewBag.Animals = _context.Animals
                .Select(a => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = a.Id.ToString(),
                    Text = $"{a.TagNumber} - {a.Type}"
                })
                .ToList();

            return View(healthRecord);
        }

        // GET: Admin/HealthRecords/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var healthRecord = await _context.HealthRecords
                .Include(h => h.Animal)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (healthRecord == null)
            {
                return NotFound();
            }

            return View(healthRecord);
        }

        // POST: Admin/HealthRecords/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var healthRecord = await _context.HealthRecords.FindAsync(id);
            if (healthRecord != null)
            {
                _context.HealthRecords.Remove(healthRecord);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool HealthRecordExists(int id)
        {
            return _context.HealthRecords.Any(e => e.Id == id);
        }
    }

    public class VaccinationScheduleViewModel
    {
        public Animal Animal { get; set; }
        public DateTime? LastVaccinationDate { get; set; }
        public DateTime NextVaccinationDue { get; set; }
        public bool IsOverdue { get; set; }
    }
} 