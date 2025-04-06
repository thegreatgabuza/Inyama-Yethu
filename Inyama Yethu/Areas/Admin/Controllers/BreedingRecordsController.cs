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
    public class BreedingRecordsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BreedingRecordsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/BreedingRecords
        public async Task<IActionResult> Index()
        {
            var matings = await _context.Matings
                .Include(m => m.Mother)
                .Include(m => m.Father)
                .OrderByDescending(m => m.MatingDate)
                .ToListAsync();

            return View(matings);
        }

        // GET: Admin/BreedingRecords/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
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

        // GET: Admin/BreedingRecords/Create
        public async Task<IActionResult> Create()
        {
            // Get female pigs (sows)
            var females = await _context.Animals
                .Where(a => a.Gender == Gender.Female && 
                           a.Type == AnimalType.Sow && 
                           a.Status == AnimalStatus.Active)
                .OrderBy(a => a.TagNumber)
                .ToListAsync();

            // Get male pigs (boars)
            var males = await _context.Animals
                .Where(a => a.Gender == Gender.Male && 
                           a.Type == AnimalType.Boar && 
                           a.Status == AnimalStatus.Active)
                .OrderBy(a => a.TagNumber)
                .ToListAsync();

            ViewData["MotherAnimalId"] = new SelectList(females, "Id", "TagNumber");
            ViewData["FatherAnimalId"] = new SelectList(males, "Id", "TagNumber");

            return View();
        }

        // POST: Admin/BreedingRecords/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MotherAnimalId,FatherAnimalId,MatingDate,Notes,Status")] Mating mating)
        {
            if (ModelState.IsValid)
            {
                // If status is not set, default to Scheduled
                if (mating.Status == 0)
                {
                    mating.Status = MatingStatus.Scheduled;
                }
                
                // Set default values based on mating date
                mating.ExpectedPregnancyCheck1 = mating.MatingDate.AddDays(21); // 3 weeks
                mating.ExpectedPregnancyCheck2 = mating.MatingDate.AddDays(42); // 6 weeks
                mating.ExpectedFarrowingDate = mating.MatingDate.AddDays(114); // ~16.5 weeks (3 months, 3 weeks, 3 days)
                mating.ExpectedVaccinationDate1 = mating.MatingDate.AddDays(100);
                mating.ExpectedVaccinationDate2 = mating.MatingDate.AddDays(107);

                _context.Add(mating);
                
                // Update the status of the mother animal
                var mother = await _context.Animals.FindAsync(mating.MotherAnimalId);
                if (mother != null)
                {
                    mother.Status = AnimalStatus.Mating;
                    mother.UpdatedAt = DateTime.Now;
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // If we got this far, something failed, redisplay form
            var females = await _context.Animals
                .Where(a => a.Gender == Gender.Female && 
                           a.Type == AnimalType.Sow && 
                           a.Status == AnimalStatus.Active)
                .OrderBy(a => a.TagNumber)
                .ToListAsync();

            var males = await _context.Animals
                .Where(a => a.Gender == Gender.Male && 
                           a.Type == AnimalType.Boar && 
                           a.Status == AnimalStatus.Active)
                .OrderBy(a => a.TagNumber)
                .ToListAsync();

            ViewData["MotherAnimalId"] = new SelectList(females, "Id", "TagNumber", mating.MotherAnimalId);
            ViewData["FatherAnimalId"] = new SelectList(males, "Id", "TagNumber", mating.FatherAnimalId);

            return View(mating);
        }

        // GET: Admin/BreedingRecords/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mating = await _context.Matings.FindAsync(id);
            if (mating == null)
            {
                return NotFound();
            }

            var females = await _context.Animals
                .Where(a => a.Gender == Gender.Female && a.Type == AnimalType.Sow)
                .OrderBy(a => a.TagNumber)
                .ToListAsync();

            var males = await _context.Animals
                .Where(a => a.Gender == Gender.Male && a.Type == AnimalType.Boar)
                .OrderBy(a => a.TagNumber)
                .ToListAsync();

            ViewData["MotherAnimalId"] = new SelectList(females, "Id", "TagNumber", mating.MotherAnimalId);
            ViewData["FatherAnimalId"] = new SelectList(males, "Id", "TagNumber", mating.FatherAnimalId);

            return View(mating);
        }

        // POST: Admin/BreedingRecords/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,MotherAnimalId,FatherAnimalId,MatingDate,Status,ExpectedPregnancyCheck1,PregnancyCheck1Result,ExpectedPregnancyCheck2,PregnancyCheck2Result,ExpectedFarrowingDate,ExpectedVaccinationDate1,Vaccination1Completed,ExpectedVaccinationDate2,Vaccination2Completed,ActualFarrowingDate,NumberOfPigletsBorn,NumberOfPigletsBornAlive,Notes")] Mating mating)
        {
            if (id != mating.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Get the original mating to check for status changes
                    var originalMating = await _context.Matings
                        .AsNoTracking()
                        .FirstOrDefaultAsync(m => m.Id == id);

                    _context.Update(mating);

                    // Handle mother status changes based on mating status
                    var mother = await _context.Animals.FindAsync(mating.MotherAnimalId);
                    if (mother != null)
                    {
                        if (mating.Status == MatingStatus.PregnancyConfirmed && originalMating.Status != MatingStatus.PregnancyConfirmed)
                        {
                            mother.Status = AnimalStatus.Pregnant;
                        }
                        else if (mating.Status == MatingStatus.Farrowed && originalMating.Status != MatingStatus.Farrowed)
                        {
                            mother.Status = AnimalStatus.Farrowing;
                        }
                        else if (mating.Status == MatingStatus.NotPregnant && originalMating.Status != MatingStatus.NotPregnant)
                        {
                            mother.Status = AnimalStatus.Active;
                        }

                        mother.UpdatedAt = DateTime.Now;
                    }

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MatingExists(mating.Id))
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

            var females = await _context.Animals
                .Where(a => a.Gender == Gender.Female && a.Type == AnimalType.Sow)
                .OrderBy(a => a.TagNumber)
                .ToListAsync();

            var males = await _context.Animals
                .Where(a => a.Gender == Gender.Male && a.Type == AnimalType.Boar)
                .OrderBy(a => a.TagNumber)
                .ToListAsync();

            ViewData["MotherAnimalId"] = new SelectList(females, "Id", "TagNumber", mating.MotherAnimalId);
            ViewData["FatherAnimalId"] = new SelectList(males, "Id", "TagNumber", mating.FatherAnimalId);

            return View(mating);
        }

        // GET: Admin/BreedingRecords/PregnancyCheck/5
        public async Task<IActionResult> PregnancyCheck(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mating = await _context.Matings
                .Include(m => m.Mother)
                .Include(m => m.Father)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (mating == null)
            {
                return NotFound();
            }

            return View(mating);
        }

        // POST: Admin/BreedingRecords/PregnancyCheck/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PregnancyCheck(int id, int checkNumber, DateTime CheckDate, string CheckType, bool Result, string PerformedBy, string Notes)
        {
            var mating = await _context.Matings.FindAsync(id);
            
            if (mating == null)
            {
                return NotFound();
            }

            if (checkNumber == 1)
            {
                mating.PregnancyCheck1Date = CheckDate;
                mating.PregnancyCheck1Result = Result;
            }
            else if (checkNumber == 2)
            {
                mating.PregnancyCheck2Date = CheckDate;
                mating.PregnancyCheck2Result = Result;
            }

            // Store who performed the check and any notes
            mating.PregnancyCheckBy = PerformedBy;
            mating.PregnancyCheckNotes = Notes;

            // Update the mating status based on the pregnancy check result
            if (!Result) // Not pregnant
            {
                mating.Status = MatingStatus.NotPregnant;
            }
            else // Confirmed pregnant
            {
                mating.Status = MatingStatus.PregnancyConfirmed;
            }

            _context.Update(mating);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/BreedingRecords/RecordFarrowing/5
        public async Task<IActionResult> RecordFarrowing(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mating = await _context.Matings
                .Include(m => m.Mother)
                .Include(m => m.Father)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (mating == null)
            {
                return NotFound();
            }

            return View(mating);
        }

        // POST: Admin/BreedingRecords/RecordFarrowing/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RecordFarrowing(int id, DateTime farrowingDate, int totalBorn, int bornAlive, string notes)
        {
            var mating = await _context.Matings.FindAsync(id);

            if (mating == null)
            {
                return NotFound();
            }

            mating.ActualFarrowingDate = farrowingDate;
            mating.NumberOfPigletsBorn = totalBorn;
            mating.NumberOfPigletsBornAlive = bornAlive;
            mating.Status = MatingStatus.Farrowed;
            mating.Notes = notes;

            // Update mother status
            var mother = await _context.Animals.FindAsync(mating.MotherAnimalId);
            if (mother != null)
            {
                mother.Status = AnimalStatus.Farrowing;
                mother.UpdatedAt = DateTime.Now;
            }

            // Create a birth record
            var birth = new Birth
            {
                AnimalId = mating.MotherAnimalId,
                BirthDate = farrowingDate,
                NumberOfOffspring = totalBorn,
                Status = BirthStatus.Normal,
                Notes = notes
            };

            _context.Update(mating);
            _context.Add(birth);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id });
        }

        // GET: Admin/BreedingRecords/SowCard/5
        public async Task<IActionResult> SowCard(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var animal = await _context.Animals
                .Include(a => a.MatingsAsMother)
                    .ThenInclude(m => m.Father)
                .Include(a => a.Births)
                .FirstOrDefaultAsync(m => m.Id == id && m.Type == AnimalType.Sow);

            if (animal == null)
            {
                return NotFound();
            }

            return View(animal);
        }

        // Helper method to check if mating exists
        private bool MatingExists(int id)
        {
            return _context.Matings.Any(e => e.Id == id);
        }
    }
} 