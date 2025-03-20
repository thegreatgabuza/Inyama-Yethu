using Inyama_Yethu.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Inyama_Yethu.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly TimeZoneInfo _southAfricaTimeZone;

        public DashboardController(ApplicationDbContext context, TimeZoneInfo timeZoneInfo)
        {
            _context = context;
            _southAfricaTimeZone = timeZoneInfo;
        }

        // GET: Dashboard
        public async Task<IActionResult> Index()
        {
            var today = TimeZoneInfo.ConvertTime(DateTime.Now, _southAfricaTimeZone).Date;

            // Get employees and their status (checked in or not)
            var employees = await _context.Employees
                .Where(e => e.IsActive)
                .Select(e => new
                {
                    Employee = e,
                    CheckedIn = e.Attendances.Any(a => a.CheckInTime.Date == today && a.CheckOutTime == null)
                })
                .ToListAsync();

            // Get today's tasks
            var todaysTasks = await _context.TaskAssignments
                .Include(t => t.Employee)
                .Include(t => t.Animal)
                .Where(t => t.DueDate.Date == today && t.Status != Models.FarmTaskStatus.Completed)
                .OrderBy(t => t.Priority)
                .ToListAsync();

            // Get upcoming tasks (next 7 days)
            var upcomingTasks = await _context.TaskAssignments
                .Include(t => t.Employee)
                .Include(t => t.Animal)
                .Where(t => t.DueDate.Date > today && 
                          t.DueDate.Date <= today.AddDays(7) && 
                          t.Status != Models.FarmTaskStatus.Completed)
                .OrderBy(t => t.DueDate)
                .ThenBy(t => t.Priority)
                .ToListAsync();

            // Get overdue tasks
            var overdueTasks = await _context.TaskAssignments
                .Include(t => t.Employee)
                .Include(t => t.Animal)
                .Where(t => t.DueDate.Date < today && t.Status != Models.FarmTaskStatus.Completed)
                .OrderBy(t => t.DueDate)
                .ToListAsync();

            // Get livestock statistics
            var livestockStats = await _context.Animals
                .Where(a => a.Status != Models.AnimalStatus.Deceased && a.Status != Models.AnimalStatus.Sold)
                .GroupBy(a => a.Type)
                .Select(g => new { Type = g.Key, Count = g.Count() })
                .ToListAsync();

            // Get upcoming matings
            var upcomingMatings = await _context.Matings
                .Include(m => m.Mother)
                .Include(m => m.Father)
                .Where(m => m.Status == Models.MatingStatus.Scheduled && m.MatingDate.Date >= today)
                .OrderBy(m => m.MatingDate)
                .Take(5)
                .ToListAsync();

            // Get upcoming farrowings
            var upcomingFarrowings = await _context.Matings
                .Include(m => m.Mother)
                .Where(m => m.Status == Models.MatingStatus.PregnancyConfirmed && 
                          m.ExpectedFarrowingDate.Date >= today)
                .OrderBy(m => m.ExpectedFarrowingDate)
                .Take(5)
                .ToListAsync();

            // Get upcoming abattoir shipments
            var upcomingShipments = await _context.AbattoirShipments
                .Where(s => s.ShipmentDate.Date >= today && s.Status != Models.ShipmentStatus.Cancelled)
                .OrderBy(s => s.ShipmentDate)
                .Take(3)
                .ToListAsync();

            // Get recent orders
            var recentOrders = await _context.Orders
                .Include(o => o.Customer)
                .Where(o => o.Status != Models.OrderStatus.Delivered && o.Status != Models.OrderStatus.Cancelled)
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .ToListAsync();

            // Pass data to view
            ViewData["Today"] = today;
            ViewData["Employees"] = employees;
            ViewData["TodaysTasks"] = todaysTasks;
            ViewData["UpcomingTasks"] = upcomingTasks;
            ViewData["OverdueTasks"] = overdueTasks;
            ViewData["LivestockStats"] = livestockStats;
            ViewData["UpcomingMatings"] = upcomingMatings;
            ViewData["UpcomingFarrowings"] = upcomingFarrowings;
            ViewData["UpcomingShipments"] = upcomingShipments;
            ViewData["RecentOrders"] = recentOrders;

            return View();
        }

        // GET: Dashboard/LivestockOverview
        public async Task<IActionResult> LivestockOverview()
        {
            // Get livestock statistics by type
            var livestockByType = await _context.Animals
                .Where(a => a.Status != Models.AnimalStatus.Deceased && a.Status != Models.AnimalStatus.Sold)
                .GroupBy(a => a.Type)
                .Select(g => new { Type = g.Key, Count = g.Count() })
                .ToListAsync();

            // Get livestock statistics by status
            var livestockByStatus = await _context.Animals
                .GroupBy(a => a.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            // Get recent farrowings
            var recentFarrowings = await _context.Matings
                .Include(m => m.Mother)
                .Where(m => m.Status == Models.MatingStatus.Farrowed && 
                          m.ActualFarrowingDate.HasValue)
                .OrderByDescending(m => m.ActualFarrowingDate)
                .Take(5)
                .ToListAsync();

            // Get pregnant sows
            var pregnantSows = await _context.Matings
                .Include(m => m.Mother)
                .Where(m => m.Status == Models.MatingStatus.PregnancyConfirmed && 
                          !m.ActualFarrowingDate.HasValue)
                .OrderBy(m => m.ExpectedFarrowingDate)
                .ToListAsync();

            // Get abattoir statistics
            var abattoirStats = await _context.AbattoirShipments
                .Where(s => s.ShipmentDate.Year == DateTime.Now.Year)
                .GroupBy(s => s.ShipmentDate.Month)
                .Select(g => new 
                {
                    Month = g.Key,
                    PigsSent = g.Sum(s => s.NumberOfPigs),
                    Revenue = g.Sum(s => s.ActualPayment ?? s.EstimatedValue)
                })
                .ToListAsync();

            ViewData["LivestockByType"] = livestockByType;
            ViewData["LivestockByStatus"] = livestockByStatus;
            ViewData["RecentFarrowings"] = recentFarrowings;
            ViewData["PregnantSows"] = pregnantSows;
            ViewData["AbattoirStats"] = abattoirStats;

            return View();
        }

        // GET: Dashboard/TaskManagement
        public async Task<IActionResult> TaskManagement()
        {
            var today = TimeZoneInfo.ConvertTime(DateTime.Now, _southAfricaTimeZone).Date;

            // Get all active tasks
            var allTasks = await _context.TaskAssignments
                .Include(t => t.Employee)
                .Include(t => t.Animal)
                .Where(t => t.Status != Models.FarmTaskStatus.Completed)
                .OrderBy(t => t.DueDate)
                .ThenBy(t => t.Priority)
                .ToListAsync();

            // Tasks grouped by category
            var tasksByCategory = allTasks
                .GroupBy(t => t.Category?.Name ?? "Uncategorized")
                .Select(g => new { Category = g.Key, Tasks = g.ToList() })
                .ToList();

            // Tasks grouped by employee
            var tasksByEmployee = allTasks
                .GroupBy(t => t.Employee)
                .Select(g => new { Employee = g.Key, Tasks = g.ToList() })
                .ToList();

            // Tasks by status
            var tasksByStatus = allTasks
                .GroupBy(t => t.Status)
                .Select(g => new { Status = g.Key, Tasks = g.ToList() })
                .ToList();

            ViewData["Today"] = today;
            ViewData["AllTasks"] = allTasks;
            ViewData["TasksByCategory"] = tasksByCategory;
            ViewData["TasksByEmployee"] = tasksByEmployee;
            ViewData["TasksByStatus"] = tasksByStatus;

            return View();
        }

        // GET: Dashboard/CustomerOverview
        public async Task<IActionResult> CustomerOverview()
        {
            // Get all active customers
            var customers = await _context.Customers
                .Where(c => c.IsActive)
                .ToListAsync();

            // Get recent orders
            var recentOrders = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                    .ThenInclude(i => i.Product)
                .OrderByDescending(o => o.OrderDate)
                .Take(10)
                .ToListAsync();

            // Get recent feedback
            var recentFeedback = await _context.Feedbacks
                .Include(f => f.Customer)
                .OrderByDescending(f => f.FeedbackDate)
                .Take(10)
                .ToListAsync();

            // Get revenue by township
            var revenueByTownship = await _context.Orders
                .Where(o => o.PaymentReceived && o.OrderDate.Year == DateTime.Now.Year)
                .GroupBy(o => o.Customer.Township)
                .Select(g => new 
                {
                    Township = g.Key,
                    Revenue = g.Sum(o => o.TotalAmount),
                    OrderCount = g.Count()
                })
                .ToListAsync();

            // Get top products
            var topProducts = await _context.OrderItems
                .Include(i => i.Product)
                .GroupBy(i => i.ProductId)
                .Select(g => new 
                {
                    Product = g.First().Product,
                    TotalQuantity = g.Sum(i => i.Quantity),
                    TotalRevenue = g.Sum(i => i.UnitPrice * i.Quantity)
                })
                .OrderByDescending(x => x.TotalQuantity)
                .Take(5)
                .ToListAsync();

            ViewData["Customers"] = customers;
            ViewData["RecentOrders"] = recentOrders;
            ViewData["RecentFeedback"] = recentFeedback;
            ViewData["RevenueByTownship"] = revenueByTownship;
            ViewData["TopProducts"] = topProducts;

            return View();
        }
    }
} 