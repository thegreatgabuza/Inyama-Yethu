using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Inyama_Yethu.Models;
using Inyama_Yethu.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Inyama_Yethu.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Administrator")]
    public class AnimalsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AnimalsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Animals
        public async Task<IActionResult> Index()
        {
            var animals = await _context.Animals
                .Include(a => a.Mother)
                .Include(a => a.Father)
                .OrderBy(a => a.Type)
                .ThenBy(a => a.TagNumber)
                .ToListAsync();

            return View(animals);
        }

        // GET: Admin/Animals/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var animal = await _context.Animals
                .Include(a => a.Mother)
                .Include(a => a.Father)
                .Include(a => a.HealthRecords)
                .Include(a => a.WeightRecords)
                .Include(a => a.Feedings)
                .Include(a => a.Offspring)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (animal == null)
            {
                return NotFound();
            }

            return View(animal);
        }

        // GET: Admin/Animals/Create
        public async Task<IActionResult> Create()
        {
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

            return View();
        }

        // POST: Admin/Animals/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TagNumber,Type,Gender,BirthDate,Status,Weight,Notes,MotherAnimalId,FatherAnimalId")] Animal animal)
        {
            if (!ModelState.IsValid)
            {
                _context.Add(animal);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
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

            return View(animal);
        }

        // GET: Admin/Animals/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var animal = await _context.Animals.FindAsync(id);
            if (animal == null)
            {
                return NotFound();
            }

            var potentialMothers = await _context.Animals
                .Where(a => a.Gender == Gender.Female && 
                           a.Type == AnimalType.Sow &&
                           a.Id != id)
                .OrderBy(a => a.TagNumber)
                .ToListAsync();

            var potentialFathers = await _context.Animals
                .Where(a => a.Gender == Gender.Male && 
                           a.Type == AnimalType.Boar &&
                           a.Id != id)
                .OrderBy(a => a.TagNumber)
                .ToListAsync();

            ViewData["MotherAnimalId"] = new SelectList(potentialMothers, "Id", "TagNumber", animal.MotherAnimalId);
            ViewData["FatherAnimalId"] = new SelectList(potentialFathers, "Id", "TagNumber", animal.FatherAnimalId);

            return View(animal);
        }

        // POST: Admin/Animals/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TagNumber,Type,Gender,BirthDate,Status,Weight,Notes,MotherAnimalId,FatherAnimalId")] Animal animal)
        {
            if (id != animal.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                try
                {
                    _context.Update(animal);
                    await _context.SaveChangesAsync();
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

            var potentialMothers = await _context.Animals
                .Where(a => a.Gender == Gender.Female && 
                           a.Type == AnimalType.Sow &&
                           a.Id != id)
                .OrderBy(a => a.TagNumber)
                .ToListAsync();

            var potentialFathers = await _context.Animals
                .Where(a => a.Gender == Gender.Male && 
                           a.Type == AnimalType.Boar &&
                           a.Id != id)
                .OrderBy(a => a.TagNumber)
                .ToListAsync();

            ViewData["MotherAnimalId"] = new SelectList(potentialMothers, "Id", "TagNumber", animal.MotherAnimalId);
            ViewData["FatherAnimalId"] = new SelectList(potentialFathers, "Id", "TagNumber", animal.FatherAnimalId);

            return View(animal);
        }

        // GET: Admin/Animals/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
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

        // POST: Admin/Animals/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var animal = await _context.Animals.FindAsync(id);
            if (animal != null)
            {
                _context.Animals.Remove(animal);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool AnimalExists(int id)
        {
            return _context.Animals.Any(e => e.Id == id);
        }

        // GET: Admin/Animals/HealthRecords/5
        public async Task<IActionResult> HealthRecords(int? id)
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

        // GET: Admin/Animals/WeightRecords/5
        public async Task<IActionResult> WeightRecords(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var animal = await _context.Animals
                .Include(a => a.WeightRecords)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (animal == null)
            {
                return NotFound();
            }

            return View(animal);
        }

        // GET: Admin/Animals/Feedings/5
        public async Task<IActionResult> Feedings(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var animal = await _context.Animals
                .Include(a => a.Feedings)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (animal == null)
            {
                return NotFound();
            }

            return View(animal);
        }
    }
} 