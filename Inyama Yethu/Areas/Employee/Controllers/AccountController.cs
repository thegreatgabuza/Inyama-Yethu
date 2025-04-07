using Inyama_Yethu.Data;
using Inyama_Yethu.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Inyama_Yethu.Areas.Employee.Controllers
{
    [Area("Employee")]
    [Authorize(Roles = "Employee")]
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly TimeZoneInfo _southAfricaTimeZone;

        public AccountController(
            UserManager<IdentityUser> userManager,
            ApplicationDbContext context,
            TimeZoneInfo timeZoneInfo)
        {
            _userManager = userManager;
            _context = context;
            _southAfricaTimeZone = timeZoneInfo;
        }

        // GET: Employee/Account/Profile
        public async Task<IActionResult> Profile()
        {
            // Get the current employee based on the logged in user
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.Email == userEmail && e.IsActive);

            if (employee == null)
            {
                return RedirectToAction("AccessDenied", "Account", new { area = "" });
            }

            // Get attendance history for this employee
            var recentAttendance = await _context.Attendances
                .Where(a => a.EmployeeId == employee.Id)
                .OrderByDescending(a => a.CheckInTime)
                .Take(5)
                .ToListAsync();

            ViewData["Employee"] = employee;
            ViewData["RecentAttendance"] = recentAttendance;

            return View(employee);
        }
    }
} 