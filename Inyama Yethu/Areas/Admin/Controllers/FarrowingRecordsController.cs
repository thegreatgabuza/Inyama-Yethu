using Inyama_Yethu.Data;
using Inyama_Yethu.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Inyama_Yethu.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Administrator")]
    public class FarrowingRecordsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FarrowingRecordsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/FarrowingRecords
        public async Task<IActionResult> Index()
        {
            var today = DateTime.Now.Date;
            var thirtyDaysAgo = today.AddDays(-30);
            var thirtyDaysFromNow = today.AddDays(30);

            // Get all farrowing records (from matings)
            var farrowingRecords = await _context.Matings
                .Include(m => m.Mother)
                .Include(m => m.Father)
                .Where(m => m.Status == MatingStatus.Farrowed || 
                           (m.Status == MatingStatus.PregnancyConfirmed && 
                            m.ExpectedFarrowingDate.HasValue))
                .OrderByDescending(m => m.ActualFarrowingDate ?? m.ExpectedFarrowingDate)
                .ToListAsync();

            // Recent farrowings (within last 30 days)
            var recentFarrowings = farrowingRecords
                .Where(m => m.Status == MatingStatus.Farrowed && 
                           m.ActualFarrowingDate.HasValue && 
                           m.ActualFarrowingDate.Value >= thirtyDaysAgo)
                .ToList();

            // Upcoming farrowings (within next 30 days)
            var upcomingFarrowings = farrowingRecords
                .Where(m => m.Status == MatingStatus.PregnancyConfirmed && 
                           m.ExpectedFarrowingDate.HasValue &&
                           m.ExpectedFarrowingDate.Value <= thirtyDaysFromNow &&
                           m.ExpectedFarrowingDate.Value >= today)
                .OrderBy(m => m.ExpectedFarrowingDate)
                .ToList();

            // Farrowing statistics
            var completedFarrowings = farrowingRecords.Where(m => m.Status == MatingStatus.Farrowed).ToList();
            var totalPigletsBorn = completedFarrowings.Sum(m => m.NumberOfPigletsBorn ?? 0);
            var totalPigletsBornAlive = completedFarrowings.Sum(m => m.NumberOfPigletsBornAlive ?? 0);
            
            var mortalityRate = totalPigletsBorn > 0 
                ? 100.0 - ((double)totalPigletsBornAlive * 100.0 / (double)totalPigletsBorn) 
                : 0.0;

            // Monthly statistics
            var monthlyStats = completedFarrowings
                .Where(m => m.ActualFarrowingDate.HasValue && m.ActualFarrowingDate.Value.Year == today.Year)
                .GroupBy(m => m.ActualFarrowingDate.Value.Month)
                .Select(g => new
                {
                    Month = g.Key,
                    Count = g.Count(),
                    TotalBorn = g.Sum(m => m.NumberOfPigletsBorn ?? 0),
                    BornAlive = g.Sum(m => m.NumberOfPigletsBornAlive ?? 0)
                })
                .OrderBy(x => x.Month)
                .ToList();

            ViewData["RecentFarrowings"] = recentFarrowings;
            ViewData["UpcomingFarrowings"] = upcomingFarrowings;
            ViewData["TotalFarrowings"] = completedFarrowings.Count;
            ViewData["TotalPigletsBorn"] = totalPigletsBorn;
            ViewData["TotalPigletsBornAlive"] = totalPigletsBornAlive;
            ViewData["MortalityRate"] = mortalityRate;
            ViewData["MonthlyStats"] = monthlyStats;

            return View(farrowingRecords);
        }

        // GET: Admin/FarrowingRecords/Details/5
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

        // GET: Admin/FarrowingRecords/Create
        public async Task<IActionResult> Create()
        {
            // Get pregnant sows
            var pregnantSows = await _context.Animals
                .Where(a => a.Gender == Gender.Female && 
                          a.Type == AnimalType.Sow && 
                          a.Status == AnimalStatus.Pregnant)
                .OrderBy(a => a.TagNumber)
                .ToListAsync();

            // Get active boars
            var activeBoars = await _context.Animals
                .Where(a => a.Gender == Gender.Male && 
                          a.Type == AnimalType.Boar && 
                          a.Status == AnimalStatus.Active)
                .OrderBy(a => a.TagNumber)
                .ToListAsync();

            ViewData["MotherAnimalId"] = new SelectList(pregnantSows, "Id", "TagNumber");
            ViewData["FatherAnimalId"] = new SelectList(activeBoars, "Id", "TagNumber");

            return View();
        }

        // POST: Admin/FarrowingRecords/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MotherAnimalId,FatherAnimalId,MatingDate,ExpectedFarrowingDate,ActualFarrowingDate,NumberOfPigletsBorn,NumberOfPigletsBornAlive,Notes")] Mating mating)
        {
            if (!ModelState.IsValid)
            {
                // Set default values for required fields that aren't in the form
                mating.PregnancyCheckBy = "N/A"; // Set default value for PregnancyCheckBy
                mating.PregnancyCheckNotes = ""; // Set default empty value for PregnancyCheckNotes
                
                // Set farrowed status if actual farrowing date is provided
                if (mating.ActualFarrowingDate.HasValue)
                {
                    mating.Status = MatingStatus.Farrowed;
                    
                    // Update mother status
                    var mother = await _context.Animals.FindAsync(mating.MotherAnimalId);
                    if (mother != null)
                    {
                        mother.Status = AnimalStatus.Farrowing;
                        mother.UpdatedAt = DateTime.Now;
                        _context.Update(mother);
                    }

                    // Create a birth record
                    var birth = new Birth
                    {
                        AnimalId = mating.MotherAnimalId,
                        BirthDate = mating.ActualFarrowingDate.Value,
                        NumberOfOffspring = mating.NumberOfPigletsBorn ?? 0,
                        Status = BirthStatus.Normal,
                        Notes = mating.Notes
                    };
                    _context.Add(birth);
                }
                else
                {
                    mating.Status = MatingStatus.PregnancyConfirmed;
                }

                _context.Add(mating);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // If we got this far, something failed, redisplay form
            var pregnantSows = await _context.Animals
                .Where(a => a.Gender == Gender.Female && 
                          a.Type == AnimalType.Sow && 
                          a.Status == AnimalStatus.Pregnant)
                .OrderBy(a => a.TagNumber)
                .ToListAsync();

            var activeBoars = await _context.Animals
                .Where(a => a.Gender == Gender.Male && 
                          a.Type == AnimalType.Boar && 
                          a.Status == AnimalStatus.Active)
                .OrderBy(a => a.TagNumber)
                .ToListAsync();

            ViewData["MotherAnimalId"] = new SelectList(pregnantSows, "Id", "TagNumber", mating.MotherAnimalId);
            ViewData["FatherAnimalId"] = new SelectList(activeBoars, "Id", "TagNumber", mating.FatherAnimalId);

            return View(mating);
        }

        // GET: Admin/FarrowingRecords/Edit/5
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

            var sows = await _context.Animals
                .Where(a => a.Gender == Gender.Female && 
                          a.Type == AnimalType.Sow)
                .OrderBy(a => a.TagNumber)
                .ToListAsync();

            var boars = await _context.Animals
                .Where(a => a.Gender == Gender.Male && 
                          a.Type == AnimalType.Boar)
                .OrderBy(a => a.TagNumber)
                .ToListAsync();

            ViewData["MotherAnimalId"] = new SelectList(sows, "Id", "TagNumber", mating.MotherAnimalId);
            ViewData["FatherAnimalId"] = new SelectList(boars, "Id", "TagNumber", mating.FatherAnimalId);

            return View(mating);
        }

        // POST: Admin/FarrowingRecords/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,MotherAnimalId,FatherAnimalId,MatingDate,Status,ExpectedFarrowingDate,ActualFarrowingDate,NumberOfPigletsBorn,NumberOfPigletsBornAlive,Notes")] Mating mating)
        {
            if (id != mating.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Get the original record to preserve any values that weren't part of the form
                    var originalMating = await _context.Matings.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);
                    
                    // Ensure required fields have values
                    if (string.IsNullOrEmpty(mating.PregnancyCheckBy))
                    {
                        mating.PregnancyCheckBy = originalMating?.PregnancyCheckBy ?? "N/A";
                    }
                    
                    if (string.IsNullOrEmpty(mating.PregnancyCheckNotes))
                    {
                        mating.PregnancyCheckNotes = originalMating?.PregnancyCheckNotes ?? "";
                    }
                    
                    // Update the mother's status based on mating status
                    var mother = await _context.Animals.FindAsync(mating.MotherAnimalId);
                    if (mother != null)
                    {
                        if (mating.Status == MatingStatus.Farrowed && mating.ActualFarrowingDate.HasValue)
                        {
                            mother.Status = AnimalStatus.Farrowing;
                        }
                        else if (mating.Status == MatingStatus.PregnancyConfirmed)
                        {
                            mother.Status = AnimalStatus.Pregnant;
                        }
                        else if (mating.Status == MatingStatus.NotPregnant || mating.Status == MatingStatus.Aborted)
                        {
                            mother.Status = AnimalStatus.Active;
                        }
                        
                        mother.UpdatedAt = DateTime.Now;
                        _context.Update(mother);
                        
                        // Create a birth record if not already exists
                        if (mating.Status == MatingStatus.Farrowed && mating.ActualFarrowingDate.HasValue)
                        {
                            // Create a birth record if not already exists
                            if (!await _context.Births.AnyAsync(b => b.AnimalId == mating.MotherAnimalId && 
                                                                   b.BirthDate.Date == mating.ActualFarrowingDate.Value.Date))
                            {
                                var birth = new Birth
                                {
                                    AnimalId = mating.MotherAnimalId,
                                    BirthDate = mating.ActualFarrowingDate.Value,
                                    NumberOfOffspring = mating.NumberOfPigletsBorn ?? 0,
                                    Status = BirthStatus.Normal,
                                    Notes = mating.Notes
                                };
                                _context.Add(birth);
                            }
                        }

                        _context.Update(mating);
                        await _context.SaveChangesAsync();
                    }
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

            var sows = await _context.Animals
                .Where(a => a.Gender == Gender.Female && 
                          a.Type == AnimalType.Sow)
                .OrderBy(a => a.TagNumber)
                .ToListAsync();

            var boars = await _context.Animals
                .Where(a => a.Gender == Gender.Male && 
                          a.Type == AnimalType.Boar)
                .OrderBy(a => a.TagNumber)
                .ToListAsync();

            ViewData["MotherAnimalId"] = new SelectList(sows, "Id", "TagNumber", mating.MotherAnimalId);
            ViewData["FatherAnimalId"] = new SelectList(boars, "Id", "TagNumber", mating.FatherAnimalId);
            
            return View(mating);
        }

        // GET: Admin/FarrowingRecords/PrintSowCard/5
        public async Task<IActionResult> PrintSowCard(int? id)
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

        // GET: Admin/FarrowingRecords/Report
        public async Task<IActionResult> Report(DateTime? startDate, DateTime? endDate)
        {
            var today = DateTime.Now.Date;
            var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);
            
            // Default to current month if no dates provided
            var reportStartDate = startDate ?? firstDayOfMonth;
            var reportEndDate = endDate ?? today;

            var farrowingRecords = await _context.Matings
                .Include(m => m.Mother)
                .Include(m => m.Father)
                .Where(m => m.Status == MatingStatus.Farrowed && 
                           m.ActualFarrowingDate.HasValue &&
                           m.ActualFarrowingDate.Value >= reportStartDate &&
                           m.ActualFarrowingDate.Value <= reportEndDate)
                .OrderByDescending(m => m.ActualFarrowingDate)
                .ToListAsync();

            // Calculate statistics
            var totalFarrowings = farrowingRecords.Count;
            var totalPigletsBorn = farrowingRecords.Sum(m => m.NumberOfPigletsBorn ?? 0);
            var totalPigletsBornAlive = farrowingRecords.Sum(m => m.NumberOfPigletsBornAlive ?? 0);
            var avgLitterSize = totalFarrowings > 0 ? (double)totalPigletsBorn / totalFarrowings : 0;
            
            ViewData["StartDate"] = reportStartDate;
            ViewData["EndDate"] = reportEndDate;
            ViewData["TotalFarrowings"] = totalFarrowings;
            ViewData["TotalPigletsBorn"] = totalPigletsBorn;
            ViewData["TotalPigletsBornAlive"] = totalPigletsBornAlive;
            ViewData["AvgLitterSize"] = avgLitterSize;

            return View(farrowingRecords);
        }

        private bool MatingExists(int id)
        {
            return _context.Matings.Any(e => e.Id == id);
        }
    }
} 