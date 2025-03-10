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
    public class AttendanceController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly TimeZoneInfo _southAfricaTimeZone;

        public AttendanceController(ApplicationDbContext context, TimeZoneInfo timeZoneInfo)
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

            // Check if employee is checked in today
            var todayAttendance = await _context.Attendances
                .FirstOrDefaultAsync(a => a.EmployeeId == employee.Id && 
                                         a.CheckInTime.Date == today);

            // Get attendance records for current month
            var startOfMonth = new DateTime(today.Year, today.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

            var monthlyAttendance = await _context.Attendances
                .Where(a => a.EmployeeId == employee.Id && 
                          a.CheckInTime >= startOfMonth && 
                          a.CheckInTime <= endOfMonth)
                .OrderByDescending(a => a.CheckInTime)
                .ToListAsync();

            // Pass data to view
            ViewData["Employee"] = employee;
            ViewData["Today"] = today;
            ViewData["TodayAttendance"] = todayAttendance;
            ViewData["MonthlyAttendance"] = monthlyAttendance;
            ViewData["StartOfMonth"] = startOfMonth;
            ViewData["EndOfMonth"] = endOfMonth;

            return View();
        }

        public async Task<IActionResult> CheckIn()
        {
            // Get the current employee based on the logged in user
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.Email == userEmail && e.IsActive);

            if (employee == null)
            {
                return RedirectToAction("AccessDenied", "Account", new { area = "" });
            }

            var now = TimeZoneInfo.ConvertTime(DateTime.Now, _southAfricaTimeZone);
            var today = now.Date;

            // Check if already checked in
            var existingAttendance = await _context.Attendances
                .FirstOrDefaultAsync(a => a.EmployeeId == employee.Id && 
                                         a.CheckInTime.Date == today && 
                                         a.CheckOutTime == null);

            if (existingAttendance != null)
            {
                TempData["ErrorMessage"] = "You have already checked in today.";
                return RedirectToAction(nameof(Index));
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
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> CheckOut()
        {
            // Get the current employee based on the logged in user
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.Email == userEmail && e.IsActive);

            if (employee == null)
            {
                return RedirectToAction("AccessDenied", "Account", new { area = "" });
            }

            var now = TimeZoneInfo.ConvertTime(DateTime.Now, _southAfricaTimeZone);

            // Find the most recent check-in without a check-out
            var attendance = await _context.Attendances
                .Where(a => a.EmployeeId == employee.Id && a.CheckOutTime == null)
                .OrderByDescending(a => a.CheckInTime)
                .FirstOrDefaultAsync();

            if (attendance == null)
            {
                TempData["ErrorMessage"] = "You are not checked in.";
                return RedirectToAction(nameof(Index));
            }

            // Update the attendance record with check-out time
            attendance.CheckOutTime = now;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Check-out successful at {now.ToString("HH:mm")}";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> History(DateTime? startDate, DateTime? endDate)
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

            // Default to current month if no dates provided
            if (!startDate.HasValue)
            {
                startDate = new DateTime(today.Year, today.Month, 1);
            }

            if (!endDate.HasValue)
            {
                endDate = today;
            }

            var attendanceHistory = await _context.Attendances
                .Where(a => a.EmployeeId == employee.Id && 
                          a.CheckInTime.Date >= startDate.Value.Date && 
                          a.CheckInTime.Date <= endDate.Value.Date)
                .OrderByDescending(a => a.CheckInTime)
                .ToListAsync();

            // Calculate statistics
            var totalHours = attendanceHistory
                .Where(a => a.CheckOutTime.HasValue)
                .Sum(a => (a.CheckOutTime.Value - a.CheckInTime).TotalHours);

            var avgHoursPerDay = attendanceHistory
                .Where(a => a.CheckOutTime.HasValue)
                .GroupBy(a => a.CheckInTime.Date)
                .Select(g => g.Sum(a => (a.CheckOutTime.Value - a.CheckInTime).TotalHours))
                .DefaultIfEmpty(0)
                .Average();

            // Pass data to view
            ViewData["Employee"] = employee;
            ViewData["StartDate"] = startDate.Value.ToString("yyyy-MM-dd");
            ViewData["EndDate"] = endDate.Value.ToString("yyyy-MM-dd");
            ViewData["TotalHours"] = Math.Round(totalHours, 2);
            ViewData["AvgHoursPerDay"] = Math.Round(avgHoursPerDay, 2);

            return View(attendanceHistory);
        }
    }
} 