using Inyama_Yethu.Data;
using Inyama_Yethu.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Inyama_Yethu.Areas.Employee.Controllers
{
    [Area("Employee")]
    [Authorize(Roles = "Employee")]
    public class TasksController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly TimeZoneInfo _southAfricaTimeZone;

        public TasksController(ApplicationDbContext context, TimeZoneInfo timeZoneInfo)
        {
            _context = context;
            _southAfricaTimeZone = timeZoneInfo;
        }

        // GET: Employee/Tasks
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

            // Get all tasks for this employee
            var tasks = await _context.TaskAssignments
                .Include(t => t.Animal)
                .Where(t => t.EmployeeId == employee.Id)
                .OrderBy(t => t.Status) // Show incomplete tasks first
                .ThenBy(t => t.DueDate)
                .ToListAsync();

            // Categorize tasks
            var todaysTasks = tasks
                .Where(t => t.DueDate.Date == today && t.Status != FarmTaskStatus.Completed)
                .ToList();

            var upcomingTasks = tasks
                .Where(t => t.DueDate.Date > today && t.Status != FarmTaskStatus.Completed)
                .ToList();

            var overdueTasks = tasks
                .Where(t => t.DueDate.Date < today && t.Status != FarmTaskStatus.Completed)
                .ToList();

            var completedTasks = tasks
                .Where(t => t.Status == FarmTaskStatus.Completed)
                .ToList();

            // Pass data to view
            ViewData["Employee"] = employee;
            ViewData["Today"] = today;
            ViewData["TodaysTasks"] = todaysTasks;
            ViewData["UpcomingTasks"] = upcomingTasks;
            ViewData["OverdueTasks"] = overdueTasks;
            ViewData["CompletedTasks"] = completedTasks;

            return View();
        }

        // GET: Employee/Tasks/TodaysTasks
        public async Task<IActionResult> TodaysTasks()
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

            // Get today's tasks for this employee
            var todaysTasks = await _context.TaskAssignments
                .Include(t => t.Animal)
                .Where(t => t.EmployeeId == employee.Id && 
                          t.DueDate.Date == today && 
                          t.Status != FarmTaskStatus.Completed)
                .OrderBy(t => t.DueDate)
                .ToListAsync();

            // Pass data to view
            ViewData["Employee"] = employee;
            ViewData["Today"] = today;
            ViewData["TodaysTasks"] = todaysTasks;

            return View();
        }

        // GET: Employee/Tasks/UpcomingTasks
        public async Task<IActionResult> UpcomingTasks()
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

            // Get upcoming tasks for this employee
            var upcomingTasks = await _context.TaskAssignments
                .Include(t => t.Animal)
                .Where(t => t.EmployeeId == employee.Id && 
                          t.DueDate.Date > today && 
                          t.Status != FarmTaskStatus.Completed)
                .OrderBy(t => t.DueDate)
                .ToListAsync();

            // Pass data to view
            ViewData["Employee"] = employee;
            ViewData["Today"] = today;
            ViewData["UpcomingTasks"] = upcomingTasks;

            return View();
        }

        // GET: Employee/Tasks/Details/5
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

            var task = await _context.TaskAssignments
                .Include(t => t.Animal)
                .FirstOrDefaultAsync(t => t.Id == id && t.EmployeeId == employee.Id);

            if (task == null)
            {
                return NotFound();
            }

            return View(task);
        }

        // POST: Employee/Tasks/CompleteTask
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteTask(int taskId, int employeeId, string notes)
        {
            // Get the current employee based on the logged in user
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.Email == userEmail && e.IsActive);

            if (employee == null || employee.Id != employeeId)
            {
                return RedirectToAction("AccessDenied", "Account", new { area = "" });
            }

            var task = await _context.TaskAssignments.FindAsync(taskId);
            
            if (task == null || task.EmployeeId != employeeId)
            {
                return NotFound();
            }

            task.Status = FarmTaskStatus.Completed;
            task.CompletionDate = TimeZoneInfo.ConvertTime(DateTime.Now, _southAfricaTimeZone);
            
            if (!string.IsNullOrEmpty(notes))
            {
                task.Notes = notes;
            }

            await _context.SaveChangesAsync();

            // Add success message
            TempData["SuccessMessage"] = $"Task '{task.Title}' marked as completed.";

            // Redirect back to the referring page, or to the tasks index
            var referer = Request.Headers["Referer"].ToString();
            if (!string.IsNullOrEmpty(referer))
            {
                return Redirect(referer);
            }
            
            return RedirectToAction(nameof(Index));
        }
    }
} 