using Inyama_Yethu.Data;
using Inyama_Yethu.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Inyama_Yethu.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Administrator")]
    public class TaskCategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TaskCategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/TaskCategories
        public async Task<IActionResult> Index()
        {
            return View(await _context.TaskCategories.ToListAsync());
        }

        // GET: Admin/TaskCategories/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/TaskCategories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description")] TaskCategory taskCategory)
        {
            if (ModelState.IsValid)
            {
                taskCategory.IsSystem = false;
                taskCategory.IsDefault = false;
                _context.Add(taskCategory);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Category created successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(taskCategory);
        }

        // GET: Admin/TaskCategories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var taskCategory = await _context.TaskCategories.FindAsync(id);
            if (taskCategory == null)
            {
                return NotFound();
            }

            if (taskCategory.IsSystem)
            {
                TempData["ErrorMessage"] = "System categories cannot be edited.";
                return RedirectToAction(nameof(Index));
            }

            return View(taskCategory);
        }

        // POST: Admin/TaskCategories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,IsDefault")] TaskCategory taskCategory)
        {
            if (id != taskCategory.Id)
            {
                return NotFound();
            }

            var existingCategory = await _context.TaskCategories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
            if (existingCategory.IsSystem)
            {
                TempData["ErrorMessage"] = "System categories cannot be edited.";
                return RedirectToAction(nameof(Index));
            }

            if (ModelState.IsValid)
            {
                try
                {
                    taskCategory.IsSystem = existingCategory.IsSystem;
                    _context.Update(taskCategory);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Category updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TaskCategoryExists(taskCategory.Id))
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
            return View(taskCategory);
        }

        // POST: Admin/TaskCategories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var taskCategory = await _context.TaskCategories.FindAsync(id);
            
            if (taskCategory.IsSystem)
            {
                TempData["ErrorMessage"] = "System categories cannot be deleted.";
                return RedirectToAction(nameof(Index));
            }

            // Check if category is in use
            var isInUse = await _context.TaskAssignments.AnyAsync(t => t.TaskCategoryId == id);
            if (isInUse)
            {
                TempData["ErrorMessage"] = "Category cannot be deleted because it is being used by existing tasks.";
                return RedirectToAction(nameof(Index));
            }

            _context.TaskCategories.Remove(taskCategory);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Category deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        private bool TaskCategoryExists(int id)
        {
            return _context.TaskCategories.Any(e => e.Id == id);
        }
    }
} 