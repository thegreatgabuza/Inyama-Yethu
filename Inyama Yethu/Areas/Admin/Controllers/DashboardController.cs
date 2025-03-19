using Inyama_Yethu.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Inyama_Yethu.Models;
using System.Collections.Generic;

namespace Inyama_Yethu.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Administrator")]
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
            var today = TimeZoneInfo.ConvertTime(DateTime.Now, _southAfricaTimeZone).Date;

            // Get total animals count and growth
            var totalAnimals = await _context.Animals
                .Where(a => a.Status != AnimalStatus.Deceased && a.Status != AnimalStatus.Sold)
                .CountAsync();
            
            var lastMonthAnimals = await _context.Animals
                .Where(a => a.Status != AnimalStatus.Deceased && 
                           a.Status != AnimalStatus.Sold &&
                           a.CreatedAt <= today.AddMonths(-1))
                .CountAsync();

            var growthPercentage = lastMonthAnimals > 0 
                ? ((totalAnimals - lastMonthAnimals) * 100 / lastMonthAnimals)
                : 0;

            ViewData["TotalAnimals"] = totalAnimals;
            ViewData["AnimalGrowth"] = growthPercentage;

            // Get healthy animals count
            var healthyAnimals = await _context.Animals
                .Where(a => a.Status != AnimalStatus.Deceased && 
                           a.Status != AnimalStatus.Sold &&
                           !a.HealthRecords.Any(hr => !hr.FollowUpCompleted && hr.FollowUpDate >= today))
                .CountAsync();
            ViewData["HealthyAnimals"] = healthyAnimals;
            ViewData["HealthyPercentage"] = totalAnimals > 0 ? (healthyAnimals * 100 / totalAnimals) : 0;

            // Get animals needing attention
            var animalsNeedingAttention = await _context.Animals
                .Where(a => a.Status != AnimalStatus.Deceased && 
                           a.Status != AnimalStatus.Sold &&
                           a.HealthRecords.Any(hr => !hr.FollowUpCompleted && hr.FollowUpDate >= today))
                .CountAsync();
            ViewData["AnimalsNeedingAttention"] = animalsNeedingAttention;

            // Get vaccination statistics
            var vaccinated = await _context.Animals
                .Where(a => a.Status != AnimalStatus.Deceased && 
                           a.Status != AnimalStatus.Sold &&
                           a.HealthRecords.Any(hr => 
                               hr.RecordType == HealthRecordType.Vaccination && 
                               hr.RecordDate >= today.AddMonths(-6)))
                .CountAsync();
            ViewData["VaccinationPercentage"] = totalAnimals > 0 ? (vaccinated * 100 / totalAnimals) : 0;

            // Get monthly population data for the last 12 months
            var monthlyData = new List<object>();
            var endDate = today;
            var startDate = today.AddMonths(-12);

            // Get all relevant animal data in one query
            var animalChanges = await _context.Animals
                .Where(a => a.CreatedAt <= endDate)
                .Select(a => new
                {
                    a.CreatedAt,
                    a.UpdatedAt,
                    IsActive = a.Status != AnimalStatus.Deceased && a.Status != AnimalStatus.Sold
                })
                .ToListAsync();

            // Calculate initial population
            var initialPopulation = animalChanges.Count(a => 
                a.CreatedAt < startDate && a.IsActive);

            // Calculate monthly statistics
            for (int i = 0; i <= 12; i++)
            {
                var currentDate = startDate.AddMonths(i);
                var nextDate = i < 12 ? startDate.AddMonths(i + 1) : endDate.AddDays(1);

                // Calculate monthly changes
                var additions = animalChanges.Count(a => 
                    a.CreatedAt >= currentDate && a.CreatedAt < nextDate);

                var removals = animalChanges.Count(a => 
                    a.UpdatedAt >= currentDate && a.UpdatedAt < nextDate && !a.IsActive);

                // Update running total
                initialPopulation = initialPopulation + additions - removals;

                // Add data point with detailed information
                monthlyData.Add(new
                {
                    label = currentDate.ToString("MMM yyyy"),
                    value = initialPopulation,
                    date = currentDate.ToString("yyyy-MM-dd"),
                    additions = additions,
                    removals = removals,
                    month = currentDate.Month,
                    year = currentDate.Year
                });
            }

            ViewData["MonthlyPopulation"] = monthlyData;

            // Also store raw data for potential client-side calculations
            ViewData["PopulationRawData"] = animalChanges
                .Select(a => new
                {
                    createdAt = a.CreatedAt.ToString("yyyy-MM-dd"),
                    updatedAt = a.UpdatedAt.ToString("yyyy-MM-dd"),
                    isActive = a.IsActive
                });

            // Get health status distribution
            var healthStatusData = await _context.Animals
                .Where(a => a.Status != AnimalStatus.Deceased && a.Status != AnimalStatus.Sold)
                .Select(a => new
                {
                    Status = a.HealthRecords
                        .Where(hr => !hr.FollowUpCompleted)
                        .OrderByDescending(hr => hr.FollowUpDate)
                        .FirstOrDefault() == null ? "Healthy" :
                        (a.HealthRecords
                            .Where(hr => !hr.FollowUpCompleted)
                            .OrderByDescending(hr => hr.FollowUpDate)
                            .First().FollowUpDate < today ? "Critical" : "Under Treatment")
                })
                .GroupBy(x => x.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            ViewData["HealthStatusDistribution"] = healthStatusData;

            // Get recent activities with more details
            var recentActivities = new List<dynamic>();
            
            // Add recent health records
            var recentHealthRecords = await _context.HealthRecords
                .Include(hr => hr.Animal)
                .OrderByDescending(hr => hr.RecordDate)
                .Take(5)
                .Select(hr => new
                {
                    Title = $"Health record created for {hr.Animal.TagNumber}",
                    Timestamp = hr.RecordDate,
                    Type = "Health Record",
                    Status = hr.FollowUpCompleted ? "Completed" : 
                            (hr.FollowUpDate < today ? "Overdue" : "Pending")
                })
                .ToListAsync();
            recentActivities.AddRange(recentHealthRecords);

            // Add recent vaccinations
            var recentVaccinations = await _context.HealthRecords
                .Include(hr => hr.Animal)
                .Where(hr => hr.RecordType == HealthRecordType.Vaccination)
                .OrderByDescending(hr => hr.RecordDate)
                .Take(5)
                .Select(hr => new
                {
                    Title = $"Vaccination administered to {hr.Animal.TagNumber}",
                    Timestamp = hr.RecordDate,
                    Type = "Vaccination",
                    Status = "Completed"
                })
                .ToListAsync();
            recentActivities.AddRange(recentVaccinations);

            // Sort all activities by timestamp
            ViewData["RecentActivities"] = recentActivities
                .OrderByDescending(a => a.Timestamp)
                .Take(10)
                .ToList();

            return View();
        }

        [HttpGet]
        public async Task<JsonResult> GetFilteredData(string timeRange)
        {
            var today = TimeZoneInfo.ConvertTime(DateTime.Now, _southAfricaTimeZone).Date;
            var startDate = timeRange switch
            {
                "week" => today.AddDays(-7),
                "month" => today.AddMonths(-1),
                "year" => today.AddYears(-1),
                _ => today.AddMonths(-1)
            };

            var data = await _context.Animals
                .Where(a => a.CreatedAt >= startDate)
                .GroupBy(a => a.CreatedAt.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .OrderBy(x => x.Date)
                .ToListAsync();

            return Json(data);
        }

        [HttpGet]
        public async Task<JsonResult> GetHealthStatusData()
        {
            var today = TimeZoneInfo.ConvertTime(DateTime.Now, _southAfricaTimeZone).Date;
            
            var data = await _context.Animals
                .Where(a => a.Status != AnimalStatus.Deceased && a.Status != AnimalStatus.Sold)
                .Select(a => new
                {
                    Status = a.HealthRecords
                        .Where(hr => !hr.FollowUpCompleted)
                        .OrderByDescending(hr => hr.FollowUpDate)
                        .FirstOrDefault() == null ? "Healthy" :
                        (a.HealthRecords
                            .Where(hr => !hr.FollowUpCompleted)
                            .OrderByDescending(hr => hr.FollowUpDate)
                            .First().FollowUpDate < today ? "Critical" : "Under Treatment")
                })
                .GroupBy(x => x.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            return Json(data);
        }

        public IActionResult Reports()
        {
            return View();
        }

        public IActionResult Settings()
        {
            return View();
        }

        // Helper method for date range
        private IEnumerable<DateTime> EachDay(DateTime from, DateTime thru)
        {
            for (var day = from.Date; day.Date <= thru.Date; day = day.AddDays(1))
                yield return day;
        }
    }

    public static class DateTimeExtensions
    {
        public static DateTime StartOfMonth(this DateTime date)
        {
            return new DateTime(date.Year, date.Month, 1);
        }

        public static DateTime EndOfMonth(this DateTime date)
        {
            return date.StartOfMonth().AddMonths(1).AddDays(-1);
        }
    }
} 