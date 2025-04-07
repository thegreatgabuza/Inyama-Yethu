using System;
using System.Linq;
using System.Threading.Tasks;
using Inyama_Yethu.Data;
using Inyama_Yethu.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Inyama_Yethu.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Administrator")]
    public class ActivityLogsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly TimeZoneInfo _southAfricaTimeZone;

        public ActivityLogsController(ApplicationDbContext context, TimeZoneInfo timeZoneInfo)
        {
            _context = context;
            _southAfricaTimeZone = timeZoneInfo;
        }

        // GET: Admin/ActivityLogs
        public async Task<IActionResult> Index(string entityName = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.ActivityLogs
                .Include(a => a.Employee)
                .Include(a => a.User)
                .OrderByDescending(a => a.Timestamp)
                .AsQueryable();

            // Apply filters if provided
            if (!string.IsNullOrEmpty(entityName))
            {
                query = query.Where(a => a.EntityName == entityName);
            }

            if (startDate.HasValue)
            {
                query = query.Where(a => a.Timestamp >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                // Include the entire end date by setting time to end of day
                var endOfDay = endDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(a => a.Timestamp <= endOfDay);
            }

            // Load entity types for filter dropdown
            ViewBag.EntityNames = await _context.ActivityLogs
                .Select(a => a.EntityName)
                .Distinct()
                .OrderBy(n => n)
                .ToListAsync();

            // Set default date range if not provided
            var today = TimeZoneInfo.ConvertTime(DateTime.Now, _southAfricaTimeZone).Date;
            
            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd") ?? today.AddDays(-30).ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd") ?? today.ToString("yyyy-MM-dd");
            ViewBag.EntityName = entityName;

            return View(await query.ToListAsync());
        }

        // GET: Admin/ActivityLogs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var activityLog = await _context.ActivityLogs
                .Include(a => a.Employee)
                .Include(a => a.User)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (activityLog == null)
            {
                return NotFound();
            }

            return View(activityLog);
        }

        // GET: Admin/ActivityLogs/AnimalHistory/5
        public async Task<IActionResult> AnimalHistory(int id)
        {
            var animal = await _context.Animals.FindAsync(id);
            if (animal == null)
            {
                return NotFound();
            }

            var activityLogs = await _context.ActivityLogs
                .Include(a => a.Employee)
                .Include(a => a.User)
                .Where(a => a.EntityName == "Animal" && a.EntityId == id)
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();

            ViewBag.Animal = animal;
            
            return View(activityLogs);
        }
    }
} 