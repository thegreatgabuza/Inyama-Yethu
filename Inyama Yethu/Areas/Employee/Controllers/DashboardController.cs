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
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly TimeZoneInfo _southAfricaTimeZone;

        public DashboardController(ApplicationDbContext context, TimeZoneInfo timeZoneInfo)
        {
            _context = context;
            _southAfricaTimeZone = timeZoneInfo;
        }

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
            
            // Calculate dates for queries instead of using TotalDays
            var fourteenDaysAgo = today.AddDays(-14);
            var fourteenDaysFromNow = today.AddDays(14);

            // Check if employee is checked in today
            var isCheckedIn = await _context.Attendances
                .AnyAsync(a => a.EmployeeId == employee.Id && 
                              a.CheckInTime.Date == today && 
                              a.CheckOutTime == null);

            // Get today's tasks for this employee
            var todaysTasks = await _context.TaskAssignments
                .Include(t => t.Animal)
                .Where(t => t.EmployeeId == employee.Id && 
                          t.DueDate.Date == today && 
                          t.Status != FarmTaskStatus.Completed)
                .OrderBy(t => t.Priority)
                .ToListAsync();

            // Get overdue tasks for this employee
            var overdueTasks = await _context.TaskAssignments
                .Include(t => t.Animal)
                .Where(t => t.EmployeeId == employee.Id && 
                          t.DueDate.Date < today && 
                          t.Status != FarmTaskStatus.Completed)
                .OrderBy(t => t.DueDate)
                .ToListAsync();

            // Get upcoming tasks for this employee
            var upcomingTasks = await _context.TaskAssignments
                .Include(t => t.Animal)
                .Where(t => t.EmployeeId == employee.Id && 
                          t.DueDate.Date > today && 
                          t.Status != FarmTaskStatus.Completed)
                .OrderBy(t => t.DueDate)
                .Take(5)
                .ToListAsync();

            // Get recent attendance records
            var recentAttendance = await _context.Attendances
                .Where(a => a.EmployeeId == employee.Id)
                .OrderByDescending(a => a.CheckInTime)
                .Take(5)
                .ToListAsync();
                
            // Get counts for farm activity overview - fix the queries to avoid TotalDays calculation
            var unprocessedPigletsCount = await _context.Animals
                .Where(a => a.Type == AnimalType.Piglet && 
                          a.BirthDate >= fourteenDaysAgo)
                .CountAsync();
                
            var upcomingFarrowingsCount = await _context.Matings
                .Where(m => m.Status == MatingStatus.PregnancyConfirmed &&
                          m.ExpectedFarrowingDate <= fourteenDaysFromNow &&
                          m.ExpectedFarrowingDate >= today)
                .CountAsync();
                
            var recentFarrowingsCount = await _context.Matings
                .Where(m => m.Status == MatingStatus.Farrowed &&
                          m.ActualFarrowingDate.HasValue &&
                          m.ActualFarrowingDate.Value >= fourteenDaysAgo)
                .CountAsync();

            // Pass data to view
            ViewData["Employee"] = employee;
            ViewData["Today"] = today;
            ViewData["IsCheckedIn"] = isCheckedIn;
            ViewData["TodaysTasks"] = todaysTasks;
            ViewData["OverdueTasks"] = overdueTasks;
            ViewData["UpcomingTasks"] = upcomingTasks;
            ViewData["RecentAttendance"] = recentAttendance;
            ViewData["UnprocessedPigletsCount"] = unprocessedPigletsCount;
            ViewData["UpcomingFarrowingsCount"] = upcomingFarrowingsCount;
            ViewData["RecentFarrowingsCount"] = recentFarrowingsCount;

            return View();
        }

        // Add error handling for invalid routes
        [Route("Employee/Dashboard/Error")]
        public IActionResult Error()
        {
            return View("Error");
        }
    }
} 