using Inyama_Yethu.Data;
using Inyama_Yethu.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Inyama_Yethu.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Administrator")]
    public class FeedManagementController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FeedManagementController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/FeedManagement
        public async Task<IActionResult> Index()
        {
            var today = DateTime.Now.Date;
            var startOfMonth = new DateTime(today.Year, today.Month, 1);
            var startOfPrevMonth = startOfMonth.AddMonths(-1);

            // Get all feedings ordered by date
            var feedings = await _context.Feedings
                .Include(f => f.Animal)
                .OrderByDescending(f => f.FeedDate)
                .ToListAsync();

            // Get this month's feed consumption by type
            var thisMonthConsumption = feedings
                .Where(f => f.FeedDate >= startOfMonth)
                .GroupBy(f => f.FeedType)
                .Select(g => new
                {
                    FeedType = g.Key,
                    TotalQuantity = g.Sum(f => f.Quantity),
                    TotalCost = g.Sum(f => f.Quantity * (double)f.CostPerKg)
                })
                .OrderByDescending(x => x.TotalQuantity)
                .ToList();

            // Get previous month's feed consumption for comparison
            var prevMonthConsumption = feedings
                .Where(f => f.FeedDate >= startOfPrevMonth && f.FeedDate < startOfMonth)
                .GroupBy(f => f.FeedType)
                .Select(g => new
                {
                    FeedType = g.Key,
                    TotalQuantity = g.Sum(f => f.Quantity),
                    TotalCost = g.Sum(f => f.Quantity * (double)f.CostPerKg)
                })
                .ToDictionary(x => x.FeedType, x => x);

            // Calculate totals
            var totalFeedThisMonth = thisMonthConsumption.Sum(c => c.TotalQuantity);
            var totalCostThisMonth = thisMonthConsumption.Sum(c => c.TotalCost);
            
            var totalFeedPrevMonth = prevMonthConsumption.Values.Sum(c => c.TotalQuantity);
            var totalCostPrevMonth = prevMonthConsumption.Values.Sum(c => c.TotalCost);

            // Calculate changes from previous month
            var quantityChange = totalFeedPrevMonth > 0 
                ? (totalFeedThisMonth - totalFeedPrevMonth) / totalFeedPrevMonth * 100 
                : 0;
            
            var costChange = totalCostPrevMonth > 0 
                ? (totalCostThisMonth - totalCostPrevMonth) / totalCostPrevMonth * 100 
                : 0;

            // Get feed usage by animal type
            var feedByAnimalType = feedings
                .Where(f => f.FeedDate >= startOfMonth)
                .Join(_context.Animals, 
                      f => f.AnimalId, 
                      a => a.Id, 
                      (f, a) => new { Feeding = f, Animal = a })
                .GroupBy(x => x.Animal.Type)
                .Select(g => new
                {
                    AnimalType = g.Key,
                    TotalQuantity = g.Sum(x => x.Feeding.Quantity),
                    TotalCost = g.Sum(x => x.Feeding.Quantity * (double)x.Feeding.CostPerKg)
                })
                .OrderByDescending(x => x.TotalQuantity)
                .ToList();

            // Prepare monthly data for charts
            var monthlyData = feedings
                .Where(f => f.FeedDate.Year == today.Year)
                .GroupBy(f => new { Month = f.FeedDate.Month, Type = f.FeedType })
                .Select(g => new
                {
                    Month = g.Key.Month,
                    FeedType = g.Key.Type,
                    TotalQuantity = g.Sum(f => f.Quantity)
                })
                .ToList();

            ViewData["ThisMonthConsumption"] = thisMonthConsumption;
            ViewData["TotalFeedThisMonth"] = totalFeedThisMonth;
            ViewData["TotalCostThisMonth"] = totalCostThisMonth;
            ViewData["QuantityChange"] = quantityChange;
            ViewData["CostChange"] = costChange;
            ViewData["FeedByAnimalType"] = feedByAnimalType;
            ViewData["MonthlyData"] = monthlyData;

            return View(feedings);
        }

        // GET: Admin/FeedManagement/Inventory
        public async Task<IActionResult> Inventory()
        {
            // In a real implementation, you would have a separate FeedInventory model
            // For now, we'll simulate inventory based on purchases and usage
            
            // Get all feed types
            var feedTypes = Enum.GetValues(typeof(FeedType))
                .Cast<FeedType>()
                .Select(ft => new
                {
                    Type = ft,
                    StockOnHand = 1000, // Placeholder value
                    ReorderLevel = 200,  // Placeholder value
                    LastPurchaseDate = DateTime.Now.AddDays(-14), // Placeholder
                    LastPurchasePrice = 8.50M, // Placeholder value
                    AverageDailyUsage = 25 // Placeholder value
                })
                .ToList();

            return View(feedTypes);
        }

        // GET: Admin/FeedManagement/Record
        public IActionResult Record()
        {
            // Populate dropdown for animals
            ViewData["AnimalId"] = new SelectList(_context.Animals
                .Where(a => a.Status != AnimalStatus.Deceased && a.Status != AnimalStatus.Sold)
                .OrderBy(a => a.TagNumber), "Id", "TagNumber");
            
            return View();
        }

        // POST: Admin/FeedManagement/Record
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Record([Bind("AnimalId,FeedDate,FeedType,Quantity,CostPerKg,Notes")] Feeding feeding)
        {
            if (ModelState.IsValid)
            {
                _context.Add(feeding);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            
            ViewData["AnimalId"] = new SelectList(_context.Animals
                .Where(a => a.Status != AnimalStatus.Deceased && a.Status != AnimalStatus.Sold)
                .OrderBy(a => a.TagNumber), "Id", "TagNumber", feeding.AnimalId);
                
            return View(feeding);
        }

        // GET: Admin/FeedManagement/BatchRecord
        public IActionResult BatchRecord()
        {
            // Get all animal types for batch feeding
            var animalTypes = Enum.GetValues(typeof(AnimalType))
                                  .Cast<AnimalType>()
                                  .ToList();
                                  
            ViewData["AnimalTypes"] = animalTypes;
            
            return View();
        }

        // POST: Admin/FeedManagement/BatchRecord
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BatchRecord(AnimalType animalType, FeedType feedType, double quantityPerAnimal, decimal costPerKg, DateTime feedDate, string notes)
        {
            if (ModelState.IsValid)
            {
                // Get all animals of the specified type
                var animals = await _context.Animals
                    .Where(a => a.Type == animalType && 
                              a.Status != AnimalStatus.Deceased && 
                              a.Status != AnimalStatus.Sold)
                    .ToListAsync();

                // Create feeding records for each animal
                foreach (var animal in animals)
                {
                    var feeding = new Feeding
                    {
                        AnimalId = animal.Id,
                        FeedDate = feedDate,
                        FeedType = feedType,
                        Quantity = quantityPerAnimal,
                        CostPerKg = costPerKg,
                        Notes = notes
                    };
                    
                    _context.Add(feeding);
                }
                
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            
            // If we got this far, something failed, redisplay form
            var animalTypes = Enum.GetValues(typeof(AnimalType))
                                 .Cast<AnimalType>()
                                 .ToList();
                                 
            ViewData["AnimalTypes"] = animalTypes;
            
            return View();
        }

        // GET: Admin/FeedManagement/Report
        public async Task<IActionResult> Report(DateTime? startDate, DateTime? endDate)
        {
            var today = DateTime.Now.Date;
            var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);
            
            // Default to current month if no dates provided
            var reportStartDate = startDate ?? firstDayOfMonth;
            var reportEndDate = endDate ?? today;

            // Get feedings in the date range
            var feedings = await _context.Feedings
                .Include(f => f.Animal)
                .Where(f => f.FeedDate >= reportStartDate && f.FeedDate <= reportEndDate)
                .OrderBy(f => f.FeedDate)
                .ToListAsync();

            // Calculate summary statistics
            var feedTypeStats = feedings
                .GroupBy(f => f.FeedType)
                .Select(g => new
                {
                    FeedType = g.Key,
                    TotalQuantity = g.Sum(f => f.Quantity),
                    TotalCost = g.Sum(f => f.Quantity * (double)f.CostPerKg),
                    AverageCostPerKg = g.Average(f => (double)f.CostPerKg)
                })
                .OrderByDescending(x => x.TotalQuantity)
                .ToList();

            var animalTypeStats = feedings
                .Join(_context.Animals, 
                      f => f.AnimalId, 
                      a => a.Id, 
                      (f, a) => new { Feeding = f, Animal = a })
                .GroupBy(x => x.Animal.Type)
                .Select(g => new
                {
                    AnimalType = g.Key,
                    TotalQuantity = g.Sum(x => x.Feeding.Quantity),
                    TotalCost = g.Sum(x => x.Feeding.Quantity * (double)x.Feeding.CostPerKg),
                    AvgQuantityPerAnimal = g.Sum(x => x.Feeding.Quantity) / g.Count()
                })
                .OrderByDescending(x => x.TotalQuantity)
                .ToList();

            ViewData["StartDate"] = reportStartDate;
            ViewData["EndDate"] = reportEndDate;
            ViewData["FeedTypeStats"] = feedTypeStats;
            ViewData["AnimalTypeStats"] = animalTypeStats;
            ViewData["TotalQuantity"] = feedTypeStats.Sum(s => s.TotalQuantity);
            ViewData["TotalCost"] = feedTypeStats.Sum(s => s.TotalCost);

            return View(feedings);
        }
    }
} 