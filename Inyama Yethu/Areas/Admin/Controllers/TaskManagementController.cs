using Inyama_Yethu.Data;
using Inyama_Yethu.Models;
using Inyama_Yethu.Services;
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
    public class TaskManagementController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly TimeZoneInfo _southAfricaTimeZone;
        private readonly ITaskNotificationService _taskNotificationService;

        public TaskManagementController(
            ApplicationDbContext context,
            TimeZoneInfo timeZoneInfo,
            ITaskNotificationService taskNotificationService)
        {
            _context = context;
            _southAfricaTimeZone = timeZoneInfo;
            _taskNotificationService = taskNotificationService;
        }

        // GET: Admin/TaskManagement
        public async Task<IActionResult> Index()
        {
            var tasks = await _context.TaskAssignments
                .Include(t => t.Employee)
                .Include(t => t.Animal)
                .Include(t => t.Category)
                .OrderByDescending(t => t.Priority)
                .ThenBy(t => t.DueDate)
                .ToListAsync();

            return View(tasks);
        }

        // GET: Admin/TaskManagement/Create
        public async Task<IActionResult> Create()
        {
            ViewData["EmployeeId"] = new SelectList(await _context.Employees.Where(e => e.IsActive).ToListAsync(), "Id", "FullName");
            ViewData["TaskCategoryId"] = new SelectList(await _context.TaskCategories.ToListAsync(), "Id", "Name");
            
            // Update this to use AnimalStatus instead of IsActive
            var animals = await _context.Animals
                .Where(a => a.Status == AnimalStatus.Active)
                .Select(a => new
                {
                    Id = a.Id,
                    DisplayName = $"{a.TagNumber} - {a.Type} ({a.Gender})"
                })
                .ToListAsync();
            ViewData["AnimalId"] = new SelectList(animals, "Id", "DisplayName");
            
            return View();
        }

        // POST: Admin/TaskManagement/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Description,EmployeeId,DueDate,Priority,TaskCategoryId,AnimalId,Notes")] TaskAssignment task)
        {
            if (!ModelState.IsValid)
            {
                task.Status = FarmTaskStatus.Pending;
                _context.Add(task);
                await _context.SaveChangesAsync();

                // Get employee email for notification
                var employee = await _context.Employees.FindAsync(task.EmployeeId);
                if (employee != null)
                {
                    await _taskNotificationService.SendTaskAssignmentNotificationAsync(task, employee.Email);
                }

                TempData["SuccessMessage"] = "Task created successfully.";
                return RedirectToAction(nameof(Index));
            }

            ViewData["EmployeeId"] = new SelectList(await _context.Employees.Where(e => e.IsActive).ToListAsync(), "Id", "FullName");
            ViewData["TaskCategoryId"] = new SelectList(await _context.TaskCategories.ToListAsync(), "Id", "Name");
            
            // Update this to use AnimalStatus instead of IsActive
            var animals = await _context.Animals
                .Where(a => a.Status == AnimalStatus.Active)
                .Select(a => new
                {
                    Id = a.Id,
                    DisplayName = $"{a.TagNumber} - {a.Type} ({a.Gender})"
                })
                .ToListAsync();
            ViewData["AnimalId"] = new SelectList(animals, "Id", "DisplayName");
            
            return View(task);
        }

        // GET: Admin/TaskManagement/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var task = await _context.TaskAssignments.FindAsync(id);
            if (task == null)
            {
                return NotFound();
            }

            ViewData["EmployeeId"] = new SelectList(await _context.Employees.Where(e => e.IsActive).ToListAsync(), "Id", "FullName");
            ViewData["TaskCategoryId"] = new SelectList(await _context.TaskCategories.ToListAsync(), "Id", "Name");
            
            // Update this to use AnimalStatus instead of IsActive
            var animals = await _context.Animals
                .Where(a => a.Status == AnimalStatus.Active)
                .Select(a => new
                {
                    Id = a.Id,
                    DisplayName = $"{a.TagNumber} - {a.Type} ({a.Gender})"
                })
                .ToListAsync();
            ViewData["AnimalId"] = new SelectList(animals, "Id", "DisplayName");
            
            return View(task);
        }

        // POST: Admin/TaskManagement/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,EmployeeId,DueDate,Status,Priority,TaskCategoryId,AnimalId,Notes")] TaskAssignment task)
        {
            if (id != task.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingTask = await _context.TaskAssignments
                        .AsNoTracking()
                        .FirstOrDefaultAsync(t => t.Id == id);

                    if (existingTask == null)
                    {
                        return NotFound();
                    }

                    // Check if the task is being reassigned to a different employee
                    if (existingTask.EmployeeId != task.EmployeeId)
                    {
                        var previousEmployee = await _context.Employees.FindAsync(existingTask.EmployeeId);
                        var newEmployee = await _context.Employees.FindAsync(task.EmployeeId);

                        if (previousEmployee != null && newEmployee != null)
                        {
                            await _taskNotificationService.SendTaskReassignmentNotificationAsync(
                                task,
                                newEmployee.Email,
                                previousEmployee.Email
                            );
                        }
                    }
                    // Check if the status has changed
                    else if (existingTask.Status != task.Status)
                    {
                        var employee = await _context.Employees.FindAsync(task.EmployeeId);
                        if (employee != null)
                        {
                            await _taskNotificationService.SendTaskStatusChangeNotificationAsync(
                                task,
                                employee.Email,
                                existingTask.Status.ToString()
                            );
                        }
                    }

                    _context.Update(task);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Task updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TaskAssignmentExists(task.Id))
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

            ViewData["EmployeeId"] = new SelectList(await _context.Employees.Where(e => e.IsActive).ToListAsync(), "Id", "FullName");
            ViewData["TaskCategoryId"] = new SelectList(await _context.TaskCategories.ToListAsync(), "Id", "Name");
            
            // Update this to use AnimalStatus instead of IsActive
            var animals = await _context.Animals
                .Where(a => a.Status == AnimalStatus.Active)
                .Select(a => new
                {
                    Id = a.Id,
                    DisplayName = $"{a.TagNumber} - {a.Type} ({a.Gender})"
                })
                .ToListAsync();
            ViewData["AnimalId"] = new SelectList(animals, "Id", "DisplayName");
            
            return View(task);
        }

        private bool TaskAssignmentExists(int id)
        {
            return _context.TaskAssignments.Any(e => e.Id == id);
        }

        // GET: Admin/TaskManagement/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var task = await _context.TaskAssignments
                .Include(t => t.Employee)
                .Include(t => t.Animal)
                .Include(t => t.Category)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (task == null)
            {
                return NotFound();
            }

            return View(task);
        }

        // POST: Admin/TaskManagement/UpdateStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int taskId, FarmTaskStatus newStatus, string notes)
        {
            var task = await _context.TaskAssignments
                .Include(t => t.Employee)
                .FirstOrDefaultAsync(t => t.Id == taskId);

            if (task == null)
            {
                return NotFound();
            }

            var oldStatus = task.Status;
            task.Status = newStatus;
            task.LastUpdated = TimeZoneInfo.ConvertTime(DateTime.UtcNow, _southAfricaTimeZone);

            if (!string.IsNullOrWhiteSpace(notes))
            {
                task.Notes = string.IsNullOrEmpty(task.Notes)
                    ? notes
                    : $"{task.Notes}\n\n[{DateTime.Now:yyyy-MM-dd HH:mm}] Status changed from {oldStatus} to {newStatus}:\n{notes}";
            }

            // If task is completed, set completion date
            if (newStatus == FarmTaskStatus.Completed && !task.CompletionDate.HasValue)
            {
                task.CompletionDate = TimeZoneInfo.ConvertTime(DateTime.UtcNow, _southAfricaTimeZone);
            }

            try
            {
                await _context.SaveChangesAsync();

                // Send notification to employee about status change
                if (task.Employee != null)
                {
                    await _taskNotificationService.SendTaskStatusChangeNotificationAsync(
                        task,
                        task.Employee.Email,
                        oldStatus.ToString()
                    );
                }

                TempData["SuccessMessage"] = $"Task status updated to {newStatus}.";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TaskAssignmentExists(task.Id))
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

        // POST: Admin/TaskManagement/Complete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Complete(int id)
        {
            var task = await _context.TaskAssignments
                .Include(t => t.Employee)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (task == null)
            {
                return NotFound();
            }

            task.Status = FarmTaskStatus.Completed;
            task.CompletionDate = TimeZoneInfo.ConvertTime(DateTime.UtcNow, _southAfricaTimeZone);
            task.LastUpdated = TimeZoneInfo.ConvertTime(DateTime.UtcNow, _southAfricaTimeZone);

            try
            {
                await _context.SaveChangesAsync();

                // Send notification to employee about task completion
                if (task.Employee != null)
                {
                    await _taskNotificationService.SendTaskStatusChangeNotificationAsync(
                        task,
                        task.Employee.Email,
                        FarmTaskStatus.Pending.ToString()
                    );
                }

                TempData["SuccessMessage"] = "Task marked as completed.";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TaskAssignmentExists(task.Id))
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

        // POST: Admin/TaskManagement/Cancel/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var task = await _context.TaskAssignments
                .Include(t => t.Employee)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (task == null)
            {
                return NotFound();
            }

            task.Status = FarmTaskStatus.Cancelled;
            task.LastUpdated = TimeZoneInfo.ConvertTime(DateTime.UtcNow, _southAfricaTimeZone);

            try
            {
                await _context.SaveChangesAsync();

                // Send notification to employee about task cancellation
                if (task.Employee != null)
                {
                    await _taskNotificationService.SendTaskStatusChangeNotificationAsync(
                        task,
                        task.Employee.Email,
                        task.Status.ToString()
                    );
                }

                TempData["SuccessMessage"] = "Task cancelled successfully.";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TaskAssignmentExists(task.Id))
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
    }
} 