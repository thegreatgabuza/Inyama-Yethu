using Inyama_Yethu.Data;
using Inyama_Yethu.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Inyama_Yethu.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Administrator")]
    public class FinancialRecordsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FinancialRecordsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/FinancialRecords
        public async Task<IActionResult> Index()
        {
            var today = DateTime.Now.Date;
            var currentYear = today.Year;
            var currentMonth = today.Month;
            
            // Calculate first day of current month and year
            var startOfMonth = new DateTime(currentYear, currentMonth, 1);
            var startOfYear = new DateTime(currentYear, 1, 1);
            
            // Feed costs for current month
            var feedCosts = await _context.Feedings
                .Where(f => f.FeedDate >= startOfMonth)
                .SumAsync(f => f.Quantity * (double)f.CostPerKg);
                
            // Healthcare costs for current month
            var healthCareCosts = await _context.HealthRecords
                .Where(h => h.RecordDate >= startOfMonth)
                .SumAsync(h => (double)h.Cost);
                
            // Transport costs from abattoir shipments
            var transportCosts = await _context.AbattoirShipments
                .Where(s => s.ShipmentDate >= startOfMonth)
                .SumAsync(s => (double)(s.TransportationCost));
                
            // Sales revenue - from abattoir shipments
            var abattoirRevenue = await _context.AbattoirShipments
                .Where(s => s.ShipmentDate >= startOfMonth && s.Status == ShipmentStatus.Processed)
                .SumAsync(s => (double)(s.ActualPayment ?? s.EstimatedValue));
                
            // Sales revenue - from direct orders
            var directSalesRevenue = await _context.Orders
                .Where(o => o.OrderDate >= startOfMonth && o.PaymentReceived)
                .SumAsync(o => (double)o.TotalAmount);
                
            // Total costs
            var totalCosts = feedCosts + healthCareCosts + transportCosts;
            
            // Total revenue
            var totalRevenue = abattoirRevenue + directSalesRevenue;
            
            // Net profit
            var netProfit = totalRevenue - totalCosts;
            
            // Profit margin
            var profitMargin = totalRevenue > 0 ? (netProfit / totalRevenue) * 100 : 0;
            
            // Monthly data for charts
            var monthlyData = new List<object>();
            
            // Generate monthly financial data for the current year
            for (int month = 1; month <= 12; month++)
            {
                var startDate = new DateTime(currentYear, month, 1);
                var endDate = startDate.AddMonths(1).AddDays(-1);
                
                if (endDate > today)
                {
                    // Don't include future months
                    break;
                }
                
                // Get feed costs for the month
                var monthlyCosts = await _context.Feedings
                    .Where(f => f.FeedDate >= startDate && f.FeedDate <= endDate)
                    .SumAsync(f => f.Quantity * (double)f.CostPerKg);
                    
                monthlyCosts += await _context.HealthRecords
                    .Where(h => h.RecordDate >= startDate && h.RecordDate <= endDate)
                    .SumAsync(h => (double)h.Cost);
                    
                monthlyCosts += await _context.AbattoirShipments
                    .Where(s => s.ShipmentDate >= startDate && s.ShipmentDate <= endDate)
                    .SumAsync(s => (double)(s.TransportationCost));
                
                // Get revenue for the month
                var monthlyRevenue = await _context.AbattoirShipments
                    .Where(s => s.ShipmentDate >= startDate && s.ShipmentDate <= endDate && s.Status == ShipmentStatus.Processed)
                    .SumAsync(s => (double)(s.ActualPayment ?? s.EstimatedValue));
                    
                monthlyRevenue += await _context.Orders
                    .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate && o.PaymentReceived)
                    .SumAsync(o => (double)o.TotalAmount);
                
                // Calculate profit
                var monthlyProfit = monthlyRevenue - monthlyCosts;
                
                monthlyData.Add(new {
                    Month = month,
                    Costs = monthlyCosts,
                    Revenue = monthlyRevenue,
                    Profit = monthlyProfit
                });
            }
            
            // Cost breakdown by category
            var costBreakdown = new List<object>
            {
                new { Category = "Feed", Amount = feedCosts },
                new { Category = "Healthcare", Amount = healthCareCosts },
                new { Category = "Transport", Amount = transportCosts }
            };
            
            // Revenue breakdown by source
            var revenueBreakdown = new List<object>
            {
                new { Source = "Abattoir Sales", Amount = abattoirRevenue },
                new { Source = "Direct Sales", Amount = directSalesRevenue }
            };
            
            // Abattoir sales details
            var abattoirSales = await _context.AbattoirShipments
                .Where(s => s.ShipmentDate >= startOfMonth)
                .OrderByDescending(s => s.ShipmentDate)
                .Take(10)
                .ToListAsync();
                
            // Direct sales details
            var directSales = await _context.Orders
                .Include(o => o.Customer)
                .Where(o => o.OrderDate >= startOfMonth)
                .OrderByDescending(o => o.OrderDate)
                .Take(10)
                .ToListAsync();
            
            // Yearly totals
            var yearlyData = new {
                FeedCosts = await _context.Feedings
                    .Where(f => f.FeedDate >= startOfYear)
                    .SumAsync(f => f.Quantity * (double)f.CostPerKg),
                    
                HealthCareCosts = await _context.HealthRecords
                    .Where(h => h.RecordDate >= startOfYear)
                    .SumAsync(h => (double)h.Cost),
                    
                TransportCosts = await _context.AbattoirShipments
                    .Where(s => s.ShipmentDate >= startOfYear)
                    .SumAsync(s => (double)(s.TransportationCost)),
                    
                AbattoirRevenue = await _context.AbattoirShipments
                    .Where(s => s.ShipmentDate >= startOfYear && s.Status == ShipmentStatus.Processed)
                    .SumAsync(s => (double)(s.ActualPayment ?? s.EstimatedValue)),
                    
                DirectSalesRevenue = await _context.Orders
                    .Where(o => o.OrderDate >= startOfYear && o.PaymentReceived)
                    .SumAsync(o => (double)o.TotalAmount)
            };
            
            ViewData["FeedCosts"] = feedCosts;
            ViewData["HealthCareCosts"] = healthCareCosts;
            ViewData["TransportCosts"] = transportCosts;
            ViewData["AbattoirRevenue"] = abattoirRevenue;
            ViewData["DirectSalesRevenue"] = directSalesRevenue;
            ViewData["TotalCosts"] = totalCosts;
            ViewData["TotalRevenue"] = totalRevenue;
            ViewData["NetProfit"] = netProfit;
            ViewData["ProfitMargin"] = profitMargin;
            ViewData["MonthlyData"] = monthlyData;
            ViewData["CostBreakdown"] = costBreakdown;
            ViewData["RevenueBreakdown"] = revenueBreakdown;
            ViewData["AbattoirSales"] = abattoirSales;
            ViewData["DirectSales"] = directSales;
            ViewData["YearlyData"] = yearlyData;
            ViewData["CurrentMonth"] = today.ToString("MMMM yyyy");
            ViewData["CurrentYear"] = currentYear;
            
            return View();
        }
        
        // GET: Admin/FinancialRecords/ExpenseReport
        public async Task<IActionResult> ExpenseReport(DateTime? startDate, DateTime? endDate)
        {
            var today = DateTime.Now.Date;
            var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);
            
            // Default to current month if no dates provided
            var reportStartDate = startDate ?? firstDayOfMonth;
            var reportEndDate = endDate ?? today;
            
            // Get feed costs by type
            var feedCosts = await _context.Feedings
                .Where(f => f.FeedDate >= reportStartDate && f.FeedDate <= reportEndDate)
                .GroupBy(f => f.FeedType)
                .Select(g => new {
                    FeedType = g.Key,
                    Quantity = g.Sum(f => f.Quantity),
                    Cost = g.Sum(f => f.Quantity * (double)f.CostPerKg)
                })
                .OrderByDescending(x => x.Cost)
                .ToListAsync();
                
            // Get healthcare costs by type
            var healthcareCosts = await _context.HealthRecords
                .Where(h => h.RecordDate >= reportStartDate && h.RecordDate <= reportEndDate)
                .GroupBy(h => h.RecordType)
                .Select(g => new {
                    RecordType = g.Key,
                    Count = g.Count(),
                    Cost = g.Sum(h => (double)h.Cost)
                })
                .OrderByDescending(x => x.Cost)
                .ToListAsync();
                
            // Get transport costs
            var transportCosts = await _context.AbattoirShipments
                .Where(s => s.ShipmentDate >= reportStartDate && s.ShipmentDate <= reportEndDate)
                .Select(s => new {
                    ShipmentDate = s.ShipmentDate,
                    NumberOfPigs = s.NumberOfPigs,
                    TransportCost = (double)(s.TransportationCost)
                })
                .ToListAsync();
                
            // Other miscellaneous costs (placeholder for future expansion)
            var otherCosts = new List<object>();
            
            // Calculate totals
            var totalFeedCost = feedCosts.Sum(f => f.Cost);
            var totalHealthcareCost = healthcareCosts.Sum(h => h.Cost);
            var totalTransportCost = transportCosts.Sum(t => t.TransportCost);
            var totalOtherCost = 0.0; // Initialize with zero instead of using reflection
            
            var totalCost = totalFeedCost + totalHealthcareCost + totalTransportCost + totalOtherCost;
            
            ViewData["FeedCosts"] = feedCosts;
            ViewData["HealthcareCosts"] = healthcareCosts;
            ViewData["TransportCosts"] = transportCosts;
            ViewData["OtherCosts"] = otherCosts;
            ViewData["TotalFeedCost"] = totalFeedCost;
            ViewData["TotalHealthcareCost"] = totalHealthcareCost;
            ViewData["TotalTransportCost"] = totalTransportCost;
            ViewData["TotalOtherCost"] = totalOtherCost;
            ViewData["TotalCost"] = totalCost;
            ViewData["StartDate"] = reportStartDate;
            ViewData["EndDate"] = reportEndDate;
            
            return View();
        }
        
        // GET: Admin/FinancialRecords/RevenueReport
        public async Task<IActionResult> RevenueReport(DateTime? startDate, DateTime? endDate)
        {
            var today = DateTime.Now.Date;
            var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);
            
            // Default to current month if no dates provided
            var reportStartDate = startDate ?? firstDayOfMonth;
            var reportEndDate = endDate ?? today;
            
            // Get abattoir sales
            var abattoirSales = await _context.AbattoirShipments
                .Where(s => s.ShipmentDate >= reportStartDate && 
                          s.ShipmentDate <= reportEndDate && 
                          s.Status == ShipmentStatus.Processed)
                .Select(s => new {
                    ShipmentDate = s.ShipmentDate,
                    NumberOfPigs = s.NumberOfPigs,
                    Revenue = (double)(s.ActualPayment ?? s.EstimatedValue)
                })
                .ToListAsync();
                
            // Get direct sales by product
            var directSalesByProduct = await _context.OrderItems
                .Include(i => i.Order)
                .Include(i => i.Product)
                .Where(i => i.Order.OrderDate >= reportStartDate && 
                          i.Order.OrderDate <= reportEndDate &&
                          i.Order.PaymentReceived)
                .GroupBy(i => i.ProductId)
                .Select(g => new {
                    ProductId = g.Key,
                    ProductName = g.First().Product.Name,
                    Quantity = g.Sum(i => i.Quantity),
                    Revenue = g.Sum(i => (double)(i.UnitPrice * i.Quantity))
                })
                .OrderByDescending(x => x.Revenue)
                .ToListAsync();
                
            // Get direct sales by township
            var directSalesByTownship = await _context.Orders
                .Include(o => o.Customer)
                .Where(o => o.OrderDate >= reportStartDate && 
                          o.OrderDate <= reportEndDate &&
                          o.PaymentReceived)
                .GroupBy(o => o.Customer.Township)
                .Select(g => new {
                    Township = g.Key,
                    OrderCount = g.Count(),
                    Revenue = g.Sum(o => (double)o.TotalAmount)
                })
                .OrderByDescending(x => x.Revenue)
                .ToListAsync();
                
            // Calculate totals
            var totalAbattoirRevenue = abattoirSales.Sum(s => s.Revenue);
            var totalDirectSalesRevenue = directSalesByProduct.Sum(p => p.Revenue);
            var totalRevenue = totalAbattoirRevenue + totalDirectSalesRevenue;
            
            ViewData["AbattoirSales"] = abattoirSales;
            ViewData["DirectSalesByProduct"] = directSalesByProduct;
            ViewData["DirectSalesByTownship"] = directSalesByTownship;
            ViewData["TotalAbattoirRevenue"] = totalAbattoirRevenue;
            ViewData["TotalDirectSalesRevenue"] = totalDirectSalesRevenue;
            ViewData["TotalRevenue"] = totalRevenue;
            ViewData["StartDate"] = reportStartDate;
            ViewData["EndDate"] = reportEndDate;
            
            return View();
        }
        
        // GET: Admin/FinancialRecords/ProfitabilityReport
        public async Task<IActionResult> ProfitabilityReport(int year = 0)
        {
            var currentYear = DateTime.Now.Year;
            
            // Default to current year
            if (year == 0)
            {
                year = currentYear;
            }
            
            var startOfYear = new DateTime(year, 1, 1);
            var endOfYear = new DateTime(year, 12, 31);
            
            // Monthly data
            var monthlyData = new List<dynamic>();
            
            // Generate monthly financial data for the specified year
            for (int month = 1; month <= 12; month++)
            {
                var startDate = new DateTime(year, month, 1);
                var endDate = startDate.AddMonths(1).AddDays(-1);
                
                if (startDate > DateTime.Now)
                {
                    // Don't include future months
                    break;
                }
                
                // Get costs for the month
                var monthlyCosts = await _context.Feedings
                    .Where(f => f.FeedDate >= startDate && f.FeedDate <= endDate)
                    .SumAsync(f => f.Quantity * (double)f.CostPerKg);
                    
                monthlyCosts += await _context.HealthRecords
                    .Where(h => h.RecordDate >= startDate && h.RecordDate <= endDate)
                    .SumAsync(h => (double)h.Cost);
                    
                monthlyCosts += await _context.AbattoirShipments
                    .Where(s => s.ShipmentDate >= startDate && s.ShipmentDate <= endDate)
                    .SumAsync(s => (double)(s.TransportationCost));
                
                // Get revenue for the month
                var monthlyRevenue = await _context.AbattoirShipments
                    .Where(s => s.ShipmentDate >= startDate && s.ShipmentDate <= endDate && s.Status == ShipmentStatus.Processed)
                    .SumAsync(s => (double)(s.ActualPayment ?? s.EstimatedValue));
                    
                monthlyRevenue += await _context.Orders
                    .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate && o.PaymentReceived)
                    .SumAsync(o => (double)o.TotalAmount);
                
                // Calculate profit
                var monthlyProfit = monthlyRevenue - monthlyCosts;
                var profitMargin = monthlyRevenue > 0 ? (monthlyProfit / monthlyRevenue) * 100 : 0;
                
                dynamic monthData = new System.Dynamic.ExpandoObject();
                monthData.Month = month;
                monthData.MonthName = startDate.ToString("MMMM");
                monthData.Costs = monthlyCosts;
                monthData.Revenue = monthlyRevenue;
                monthData.Profit = monthlyProfit;
                monthData.ProfitMargin = profitMargin;
                
                monthlyData.Add(monthData);
            }
            
            // Yearly totals
            double yearlyTotalCosts = 0;
            double yearlyTotalRevenue = 0;
            double yearlyTotalProfit = 0;
            
            foreach (var data in monthlyData)
            {
                yearlyTotalCosts += data.Costs;
                yearlyTotalRevenue += data.Revenue;
                yearlyTotalProfit += data.Profit;
            }
            
            var yearlyTotals = new {
                Costs = yearlyTotalCosts,
                Revenue = yearlyTotalRevenue,
                Profit = yearlyTotalProfit
            };
            
            var yearlyProfitMargin = yearlyTotals.Revenue > 0 ? (yearlyTotals.Profit / yearlyTotals.Revenue) * 100 : 0;
            
            // Get available years for dropdown
            var availableYears = await _context.Feedings
                .Select(f => f.FeedDate.Year)
                .Distinct()
                .OrderByDescending(y => y)
                .ToListAsync();
                
            // Ensure current year is in the list
            if (!availableYears.Contains(currentYear))
            {
                availableYears.Insert(0, currentYear);
            }
            
            ViewData["MonthlyData"] = monthlyData;
            ViewData["YearlyTotals"] = yearlyTotals;
            ViewData["YearlyProfitMargin"] = yearlyProfitMargin;
            ViewData["SelectedYear"] = year;
            ViewData["AvailableYears"] = availableYears;
            
            return View();
        }
        
        // GET: Admin/FinancialRecords/AnimalSales
        public async Task<IActionResult> AnimalSales(DateTime? startDate, DateTime? endDate)
        {
            var today = DateTime.Now.Date;
            var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);
            
            // Default to current month if no dates provided
            var reportStartDate = startDate ?? firstDayOfMonth;
            var reportEndDate = endDate ?? today;
            
            // Get abattoir shipments
            var abattoirShipments = await _context.AbattoirShipments
                .Where(s => s.ShipmentDate >= reportStartDate && s.ShipmentDate <= reportEndDate)
                .OrderByDescending(s => s.ShipmentDate)
                .ToListAsync();
                
            // Calculate abattoir statistics
            var totalPigsShipped = abattoirShipments.Sum(s => s.NumberOfPigs);
            var totalAbattoirRevenue = abattoirShipments
                .Where(s => s.Status == ShipmentStatus.Processed)
                .Sum(s => (double)(s.ActualPayment ?? s.EstimatedValue));
                
            var averagePricePerPig = totalPigsShipped > 0
                ? totalAbattoirRevenue / totalPigsShipped
                : 0;
                
            // Get direct sales of animal products - assuming all products are animal products for now
            var animalProductSales = await _context.OrderItems
                .Include(i => i.Order)
                .Include(i => i.Product)
                .Where(i => i.Order.OrderDate >= reportStartDate && 
                          i.Order.OrderDate <= reportEndDate &&
                          i.Order.PaymentReceived)
                .GroupBy(i => i.ProductId)
                .Select(g => new {
                    ProductId = g.Key,
                    ProductName = g.First().Product.Name,
                    Quantity = g.Sum(i => i.Quantity),
                    Revenue = g.Sum(i => (double)(i.UnitPrice * i.Quantity)),
                    Orders = g.Count()
                })
                .OrderByDescending(x => x.Revenue)
                .ToListAsync();
                
            var totalDirectSalesRevenue = animalProductSales.Sum(p => p.Revenue);
            
            // Monthly trend data
            var monthlyTrendData = new List<object>();
            
            var groupedShipments = abattoirShipments
                .GroupBy(s => new { Month = s.ShipmentDate.Month, Year = s.ShipmentDate.Year })
                .ToList();
                
            foreach (var group in groupedShipments)
            {
                var monthYear = $"{group.Key.Year}-{group.Key.Month}";
                var monthName = new DateTime(group.Key.Year, group.Key.Month, 1).ToString("MMM yyyy");
                var pigsShipped = group.Sum(s => s.NumberOfPigs);
                var revenue = group.Where(s => s.Status == ShipmentStatus.Processed)
                                  .Sum(s => (double)(s.ActualPayment ?? s.EstimatedValue));
                                  
                monthlyTrendData.Add(new {
                    MonthYear = monthYear,
                    MonthName = monthName,
                    PigsShipped = pigsShipped,
                    Revenue = revenue
                });
            }
                
            ViewData["AbattoirShipments"] = abattoirShipments;
            ViewData["AnimalProductSales"] = animalProductSales;
            ViewData["TotalPigsShipped"] = totalPigsShipped;
            ViewData["TotalAbattoirRevenue"] = totalAbattoirRevenue;
            ViewData["AveragePricePerPig"] = averagePricePerPig;
            ViewData["TotalDirectSalesRevenue"] = totalDirectSalesRevenue;
            ViewData["MonthlyTrendData"] = monthlyTrendData;
            ViewData["StartDate"] = reportStartDate;
            ViewData["EndDate"] = reportEndDate;
            
            return View();
        }
    }
} 