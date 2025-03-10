using Inyama_Yethu.Data;
using Inyama_Yethu.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Inyama_Yethu.Controllers
{
    [Authorize]
    public class EmployeesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly TimeZoneInfo _southAfricaTimeZone;

        public EmployeesController(ApplicationDbContext context, TimeZoneInfo timeZoneInfo)
        {
            _context = context;
            _southAfricaTimeZone = timeZoneInfo;
        }

        // GET: Employees
        public async Task<IActionResult> Index()
        {
            var employees = await _context.Employees.ToListAsync();
            return View(employees);
        }

        // GET: Employees/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees
                .Include(e => e.Attendances.OrderByDescending(a => a.CheckInTime).Take(10))
                .Include(e => e.TaskAssignments.Where(t => t.Status != Models.FarmTaskStatus.Completed && t.DueDate >= DateTime.Now).OrderBy(t => t.DueDate).Take(5))
                .FirstOrDefaultAsync(m => m.Id == id);

            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }

        // GET: Employees/Create
        [Authorize(Roles = "Administrator")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Employees/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Create([Bind("Name,Email,Phone,Position,HireDate,IsActive")] Employee employee)
        {
            if (ModelState.IsValid)
            {
                _context.Add(employee);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(employee);
        }

        // GET: Employees/Edit/5
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
            {
                return NotFound();
            }
            return View(employee);
        }

        // POST: Employees/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Email,Phone,Position,HireDate,IsActive")] Employee employee)
        {
            if (id != employee.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(employee);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmployeeExists(employee.Id))
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
            return View(employee);
        }

        // GET: Employees/CheckIn/5
        public async Task<IActionResult> CheckIn(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees.FindAsync(id);
            if (employee == null || !employee.IsActive)
            {
                return NotFound();
            }

            // Check if employee already checked in today and hasn't checked out
            var now = TimeZoneInfo.ConvertTime(DateTime.Now, _southAfricaTimeZone);
            var today = now.Date;
            
            var existingAttendance = await _context.Attendances
                .Where(a => a.EmployeeId == id && 
                           a.CheckInTime.Date == today && 
                           a.CheckOutTime == null)
                .FirstOrDefaultAsync();

            if (existingAttendance != null)
            {
                TempData["ErrorMessage"] = "You have already checked in today and have not yet checked out.";
                return RedirectToAction(nameof(Details), new { id = id });
            }

            // Create new attendance record
            var attendance = new Attendance
            {
                EmployeeId = employee.Id,
                CheckInTime = now,
                Notes = "Check-in via employee portal"
            };

            _context.Attendances.Add(attendance);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Check-in successful at {now.ToString("HH:mm")}";
            return RedirectToAction(nameof(Details), new { id = id });
        }

        // GET: Employees/CheckOut/5
        public async Task<IActionResult> CheckOut(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees.FindAsync(id);
            if (employee == null || !employee.IsActive)
            {
                return NotFound();
            }

            // Find the most recent check-in without a check-out
            var now = TimeZoneInfo.ConvertTime(DateTime.Now, _southAfricaTimeZone);
            var attendance = await _context.Attendances
                .Where(a => a.EmployeeId == id && a.CheckOutTime == null)
                .OrderByDescending(a => a.CheckInTime)
                .FirstOrDefaultAsync();

            if (attendance == null)
            {
                TempData["ErrorMessage"] = "You have not checked in or have already checked out.";
                return RedirectToAction(nameof(Details), new { id = id });
            }

            // Update the attendance record with check-out time
            attendance.CheckOutTime = now;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Check-out successful at {now.ToString("HH:mm")}";
            return RedirectToAction(nameof(Details), new { id = id });
        }

        // GET: Employees/Tasks/5
        public async Task<IActionResult> Tasks(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees
                .Include(e => e.TaskAssignments)
                    .ThenInclude(t => t.Animal)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (employee == null)
            {
                return NotFound();
            }

            // Get today's tasks
            var today = TimeZoneInfo.ConvertTime(DateTime.Now, _southAfricaTimeZone).Date;
            var todaysTasks = employee.TaskAssignments
                .Where(t => t.DueDate.Date == today)
                .OrderBy(t => t.Priority)
                .ToList();

            // Get upcoming tasks
            var upcomingTasks = employee.TaskAssignments
                .Where(t => t.DueDate.Date > today && t.Status != Models.FarmTaskStatus.Completed)
                .OrderBy(t => t.DueDate)
                .ThenBy(t => t.Priority)
                .ToList();

            // Get overdue tasks
            var overdueTasks = employee.TaskAssignments
                .Where(t => t.DueDate.Date < today && t.Status != Models.FarmTaskStatus.Completed)
                .OrderBy(t => t.DueDate)
                .ToList();

            ViewData["TodaysTasks"] = todaysTasks;
            ViewData["UpcomingTasks"] = upcomingTasks;
            ViewData["OverdueTasks"] = overdueTasks;

            return View(employee);
        }

        // POST: Employees/CompleteTask
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteTask(int taskId, int employeeId, string notes)
        {
            var task = await _context.TaskAssignments.FindAsync(taskId);
            
            if (task == null || task.EmployeeId != employeeId)
            {
                return NotFound();
            }

            task.Status = Models.FarmTaskStatus.Completed;
            task.CompletionDate = TimeZoneInfo.ConvertTime(DateTime.Now, _southAfricaTimeZone);
            
            if (!string.IsNullOrEmpty(notes))
            {
                task.Notes = notes;
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Tasks), new { id = employeeId });
        }

        // GET: Employees/AttendanceReport
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> AttendanceReport(DateTime? startDate, DateTime? endDate)
        {
            var today = TimeZoneInfo.ConvertTime(DateTime.Now, _southAfricaTimeZone).Date;
            
            if (!startDate.HasValue)
            {
                startDate = today.AddDays(-30); // Default to last 30 days
            }
            
            if (!endDate.HasValue)
            {
                endDate = today;
            }

            var attendances = await _context.Attendances
                .Include(a => a.Employee)
                .Where(a => a.CheckInTime.Date >= startDate.Value.Date && 
                           a.CheckInTime.Date <= endDate.Value.Date)
                .OrderByDescending(a => a.CheckInTime)
                .ToListAsync();

            ViewData["StartDate"] = startDate.Value.ToString("yyyy-MM-dd");
            ViewData["EndDate"] = endDate.Value.ToString("yyyy-MM-dd");

            return View(attendances);
        }

        private bool EmployeeExists(int id)
        {
            return _context.Employees.Any(e => e.Id == id);
        }
    }
} 