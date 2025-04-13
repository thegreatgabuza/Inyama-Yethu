using Inyama_Yethu.Data;
using Inyama_Yethu.Models;
using Inyama_Yethu.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

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
            // Ensure the FinancialTransactions table exists
            EnsureFinancialTransactionsExists().Wait();
        }

        // Add method to ensure FinancialTransactions table exists
        private async Task EnsureFinancialTransactionsExists()
        {
            try
            {
                // Check if table exists by attempting to query it
                await _context.FinancialTransactions.FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                // If an exception occurs, the table might not exist
                try
                {
                    // Log the error
                    Console.WriteLine($"Error querying FinancialTransactions table: {ex.Message}");
                    Console.WriteLine("Attempting to create the table manually...");
                    
                    // Try to create the table manually if it doesn't exist
                    await _context.Database.ExecuteSqlRawAsync(@"
                        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES 
                                      WHERE TABLE_NAME = 'FinancialTransactions')
                        BEGIN
                            CREATE TABLE FinancialTransactions (
                                Id INT IDENTITY(1,1) PRIMARY KEY,
                                TransactionType NVARCHAR(50) NOT NULL,
                                Category NVARCHAR(100) NOT NULL,
                                Amount DECIMAL(18,2) NOT NULL,
                                Description NVARCHAR(500) NULL,
                                ReferenceNumber NVARCHAR(100) NULL,
                                TransactionDate DATETIME2 NOT NULL,
                                CreatedDate DATETIME2 NOT NULL,
                                RelatedEntityId INT NULL,
                                RelatedEntityType NVARCHAR(100) NOT NULL,
                                PaymentMethod NVARCHAR(50) NULL,
                                IsReconciled BIT NOT NULL,
                                RecordedBy NVARCHAR(256) NULL,
                                Notes NVARCHAR(1000) NULL
                            )
                        END");
                    
                    Console.WriteLine("FinancialTransactions table created successfully");
                }
                catch (Exception createEx)
                {
                    // Log the error but don't crash the application
                    Console.WriteLine($"Error creating FinancialTransactions table: {createEx.Message}");
                    Console.WriteLine($"Inner exception: {createEx.InnerException?.Message}");
                }
            }
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
        
        // GET: Admin/FinancialRecords/AnimalSales
        public async Task<IActionResult> AnimalSales(DateTime? startDate, DateTime? endDate)
        {
            // Set default date range if not provided
            startDate ??= DateTime.Today.AddMonths(-1);
            endDate ??= DateTime.Today;

            // Get abattoir shipments
            var abattoirShipments = await _context.AbattoirShipments
                .Where(s => s.ShipmentDate >= startDate && s.ShipmentDate <= endDate)
                .OrderByDescending(s => s.ShipmentDate)
                .ToListAsync();

            // Get animal sales
            var animalSales = await _context.AnimalSales
                .Include(s => s.Animal)
                .Where(s => s.SaleDate >= startDate && s.SaleDate <= endDate)
                .OrderByDescending(s => s.SaleDate)
                .ToListAsync();

            // Calculate summary statistics
            var totalPigsShipped = abattoirShipments.Sum(s => s.NumberOfPigs);
            var totalAbattoirRevenue = abattoirShipments
                .Sum(s => (double)(s.ActualPayment ?? s.EstimatedValue));
            var averagePricePerPig = totalPigsShipped > 0 
                ? totalAbattoirRevenue / totalPigsShipped 
                : 0;

            var totalDirectSalesRevenue = animalSales
                .Where(s => s.SaleType == SaleType.DirectSale)
                .Sum(s => (double)s.SalePrice);

            // Group sales by product type
            var animalProductSales = animalSales
                .GroupBy(s => s.Animal.Type)
                .Select(g => new
                {
                    ProductName = g.Key.ToString(),
                    Quantity = g.Count(),
                    Revenue = g.Sum(s => (double)s.SalePrice)
                })
                .OrderByDescending(x => x.Revenue)
                .ToList();

            // Calculate monthly trend data
            var monthlyTrendData = animalSales
                .GroupBy(s => new { s.SaleDate.Year, s.SaleDate.Month })
                .Select(g => new
                {
                    Month = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy"),
                    Revenue = g.Sum(s => (double)s.SalePrice),
                    Count = g.Count()
                })
                .OrderBy(x => x.Month)
                .ToList();

            // Set ViewData
            ViewData["StartDate"] = startDate;
            ViewData["EndDate"] = endDate;
            ViewData["AbattoirShipments"] = abattoirShipments;
            ViewData["AnimalSales"] = animalSales;
            ViewData["AnimalProductSales"] = animalProductSales;
            ViewData["TotalPigsShipped"] = totalPigsShipped;
            ViewData["TotalAbattoirRevenue"] = totalAbattoirRevenue;
            ViewData["AveragePricePerPig"] = averagePricePerPig;
            ViewData["TotalDirectSalesRevenue"] = totalDirectSalesRevenue;
            ViewData["MonthlyTrendData"] = monthlyTrendData;

            return View();
        }

        // GET: Admin/FinancialRecords/RecordAnimalSale
        public async Task<IActionResult> RecordAnimalSale()
        {
            try
            {
                // Initialize the model with default values
                var model = new Inyama_Yethu.Models.ViewModels.AnimalSaleViewModel
                {
                    SaleDate = DateTime.Today,
                    PaymentReceived = false
                };

                // Get active animals for the dropdown
                var activeAnimals = await _context.Animals
                    .Where(a => a.Status == AnimalStatus.Active)
                    .OrderBy(a => a.TagNumber)
                    .Select(a => new
                    {
                        Id = a.Id,
                        DisplayText = $"{a.TagNumber} - {a.Type} ({a.Weight:F1} kg)"
                    })
                    .ToListAsync();
                
                // Check if there are any active animals available
                if (activeAnimals == null || !activeAnimals.Any())
                {
                    // Use TempData for error messages
                    TempData["ErrorMessage"] = "No active animals available for sale. Please add animals to the inventory first.";
                    ViewBag.Animals = new SelectList(new List<SelectListItem>());
                    return View(model);
                }

                ViewBag.Animals = new SelectList(activeAnimals, "Id", "DisplayText");
                return View(model);
            }
            catch (Exception ex)
            {
                // Log the error
                Console.WriteLine($"Error loading RecordAnimalSale: {ex.Message}");
                
                // Use TempData for error messages
                TempData["ErrorMessage"] = "An error occurred while loading the form. Please try again later.";
                
                // Create an empty select list to avoid null reference exceptions
                ViewBag.Animals = new SelectList(new List<SelectListItem>());
                
                // Return the view with the default model
                return View(new Inyama_Yethu.Models.ViewModels.AnimalSaleViewModel { SaleDate = DateTime.Today });
            }
        }

        // POST: Admin/FinancialRecords/RecordAnimalSale
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RecordAnimalSale(AnimalSaleViewModel model, string receiptNumber)
        {
            try 
            {
                if (!ModelState.IsValid)
                {
                    var animal = await _context.Animals.FindAsync(model.AnimalId);
                    if (animal == null)
                    {
                        TempData["ErrorMessage"] = "Selected animal not found.";
                        await PopulateAnimalDropdowns();
                        return View(model);
                    }

                    if (animal.Status == AnimalStatus.Sold)
                    {
                        TempData["ErrorMessage"] = "This animal has already been sold.";
                        await PopulateAnimalDropdowns();
                        return View(model);
                    }

                    // Generate invoice number if not provided
                    if (string.IsNullOrEmpty(model.InvoiceNumber))
                    {
                        model.InvoiceNumber = $"INV{DateTime.Now:yyyyMMdd}-{await GetNextInvoiceNumber():D4}";
                    }

                    // Verify the invoice number is unique
                    var existingInvoice = await _context.AnimalSales
                        .FirstOrDefaultAsync(s => s.InvoiceNumber == model.InvoiceNumber);
                    
                    if (existingInvoice != null)
                    {
                        // Append a counter to make it unique
                        model.InvoiceNumber = $"{model.InvoiceNumber}-{DateTime.Now.Ticks % 1000}";
                    }

                    // Calculate price per kg safely to avoid division by zero
                    decimal? pricePerKg = null;
                    if (model.WeightAtSale.HasValue && model.WeightAtSale.Value > 0)
                    {
                        pricePerKg = Math.Round(model.SalePrice / (decimal)model.WeightAtSale.Value, 2);
                    }

                    var sale = new AnimalSale
                    {
                        AnimalId = model.AnimalId,
                        SaleDate = model.SaleDate,
                        SaleType = model.SaleType,
                        SalePrice = model.SalePrice,
                        WeightAtSale = model.WeightAtSale,
                        PricePerKg = pricePerKg,
                        BuyerName = model.BuyerName ?? string.Empty, // Ensure not null
                        InvoiceNumber = model.InvoiceNumber,
                        PaymentReceived = model.PaymentReceived,
                        PaymentDate = model.PaymentReceived ? (model.PaymentDate ?? DateTime.Now) : null,
                        Notes = model.Notes ?? string.Empty // Ensure not null
                    };

                    // Add receipt number if provided
                    if (!string.IsNullOrWhiteSpace(receiptNumber))
                    {
                        sale.Notes = $"Receipt Number: {receiptNumber}\n{sale.Notes}";
                    }

                    // Use the execution strategy to perform database operations
                    var strategy = _context.Database.CreateExecutionStrategy();
                    await strategy.ExecuteAsync(async () => 
                    {
                        // Begin transaction to ensure all operations succeed or fail together
                        using (var transaction = await _context.Database.BeginTransactionAsync())
                        {
                            try
                            {
                                _context.AnimalSales.Add(sale);
                                
                                // Update animal status
                                animal.Status = AnimalStatus.Sold;
                                _context.Animals.Update(animal);

                                // Save changes first
                                await _context.SaveChangesAsync();

                                // Record financial transaction
                                try
                                {
                                    var financialTransaction = new FinancialTransaction
                                    {
                                        TransactionType = "Income",
                                        Category = model.SaleType.ToString(),
                                        Amount = model.SalePrice,
                                        Description = $"Sale of {animal.TagNumber} - {animal.Type}",
                                        ReferenceNumber = model.InvoiceNumber ?? "None",
                                        TransactionDate = model.SaleDate,
                                        CreatedDate = DateTime.Now,
                                        Notes = string.Empty,
                                        PaymentMethod = "N/A",
                                        RecordedBy = User.Identity?.Name ?? "System",
                                        IsReconciled = false,
                                        RelatedEntityId = sale.Id,
                                        RelatedEntityType = "AnimalSale"
                                    };
                                    
                                    _context.FinancialTransactions.Add(financialTransaction);
                                    await _context.SaveChangesAsync();
                                }
                                catch (Exception ex)
                                {
                                    // Log the error but continue - don't fail the animal sale because of financial transaction
                                    Console.WriteLine($"Warning: Financial transaction recording failed: {ex.Message}");
                                }

                                // Commit the transaction
                                await transaction.CommitAsync();
                            }
                            catch (Exception)
                            {
                                // Roll back the transaction
                                await transaction.RollbackAsync();
                                throw; // Re-throw to be caught by outer catch block
                            }
                        }
                    });
                    
                    TempData["SuccessMessage"] = $"Sale for animal {animal.TagNumber} successfully recorded.";
                    return RedirectToAction(nameof(AnimalSales));
                }
                else
                {
                    // If model state is invalid, manually collect error messages to use with TempData
                    var errorMessages = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                        
                    if (errorMessages.Any())
                    {
                        TempData["ErrorMessage"] = string.Join("<br>", errorMessages);
                    }
                }

                // If we got this far, something failed; redisplay form
                await PopulateAnimalDropdowns();
                return View(model);
            }
            catch (DbUpdateException dbEx)
            {
                // Database-specific exceptions
                Console.WriteLine($"Database error saving sale: {dbEx.Message}");
                Console.WriteLine($"Inner exception: {dbEx.InnerException?.Message}");
                
                TempData["ErrorMessage"] = "Database error: " + (dbEx.InnerException?.Message ?? dbEx.Message);
                
                await PopulateAnimalDropdowns();
                return View(model);
            }
            catch (Exception ex)
            {
                // Handle unexpected errors
                Console.WriteLine($"Error in RecordAnimalSale POST: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                
                TempData["ErrorMessage"] = $"An unexpected error occurred: {ex.Message}. Please try again later.";
                
                await PopulateAnimalDropdowns();
                return View(model);
            }
        }

        private async Task PopulateAnimalDropdowns()
        {
            try
            {
                // For new sales, only show active animals
                // For editing, also include sold animals
                var animals = await _context.Animals
                    .Where(a => a.Status == AnimalStatus.Active) // Only active animals can be sold
                    .OrderBy(a => a.TagNumber)
                    .Select(a => new SelectListItem
                    {
                        Value = a.Id.ToString(),
                        Text = $"{a.TagNumber} - {a.Type} ({a.Status}) - {a.Weight:F1} kg"
                    })
                    .ToListAsync();

                // Show a helpful message if no animals are available
                if (animals.Count == 0)
                {
                    TempData["ErrorMessage"] = "No active animals available for sale. Please add animals to the inventory first.";
                    Console.WriteLine("No active animals found in the database.");
                }

                ViewBag.Animals = new SelectList(animals, "Value", "Text");
                
                // Debug info
                Console.WriteLine($"Populated {animals.Count} animals in dropdown");
            }
            catch (Exception ex)
            {
                // Log the error
                Console.WriteLine($"Error in PopulateAnimalDropdowns: {ex.Message}");
                
                // Create an empty select list to avoid null reference exceptions
                ViewBag.Animals = new SelectList(new List<SelectListItem>());
            }
        }

        // GET: Admin/FinancialRecords/EditAnimalSale
        public async Task<IActionResult> EditAnimalSale(int animalId)
        {
            var animal = await _context.Animals
                .Include(a => a.Sale)
                .FirstOrDefaultAsync(a => a.Id == animalId);

            if (animal == null)
            {
                return NotFound();
            }

            var model = new AnimalSaleViewModel
            {
                AnimalId = animal.Id,
            };

            if (animal.Sale != null)
            {
                // Populate model with existing sale data
                model.SaleDate = animal.Sale.SaleDate;
                model.SaleType = animal.Sale.SaleType;
                model.SalePrice = animal.Sale.SalePrice;
                model.WeightAtSale = animal.Sale.WeightAtSale;
                model.BuyerName = animal.Sale.BuyerName;
                model.InvoiceNumber = animal.Sale.InvoiceNumber;
                model.PaymentReceived = animal.Sale.PaymentReceived;
                model.PaymentDate = animal.Sale.PaymentDate;
                model.Notes = animal.Sale.Notes;
            }
            else
            {
                // Set default values for new sale
                model.SaleDate = DateTime.Today;
                model.WeightAtSale = animal.Weight;
                model.PaymentReceived = false;
            }

            await PopulateAnimalDropdowns();
            return View("RecordAnimalSale", model);
        }

        // POST: Admin/FinancialRecords/EditAnimalSale
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAnimalSale(int animalId, AnimalSaleViewModel model, string receiptNumber)
        {
            if (ModelState.IsValid)
            {
                var animal = await _context.Animals
                    .Include(a => a.Sale)
                    .FirstOrDefaultAsync(a => a.Id == animalId);

                if (animal == null)
                {
                    return NotFound();
                }

                string existingReceiptInfo = "";
                
                // Extract existing receipt number if present
                if (animal.Sale != null && animal.Sale.Notes.Contains("Receipt Number:"))
                {
                    var lines = animal.Sale.Notes.Split('\n');
                    foreach (var line in lines)
                    {
                        if (line.StartsWith("Receipt Number:"))
                        {
                            existingReceiptInfo = line;
                            break;
                        }
                    }
                }

                // Process notes to handle receipt number
                string updatedNotes = model.Notes ?? "";
                
                // Add or update receipt number
                if (!string.IsNullOrWhiteSpace(receiptNumber))
                {
                    if (string.IsNullOrEmpty(existingReceiptInfo))
                    {
                        // Add new receipt info
                        updatedNotes = $"Receipt Number: {receiptNumber}\n{updatedNotes}";
                    }
                    else
                    {
                        // Replace existing receipt info
                        updatedNotes = updatedNotes.Replace(existingReceiptInfo, $"Receipt Number: {receiptNumber}");
                        if (!updatedNotes.Contains("Receipt Number:"))
                        {
                            updatedNotes = $"Receipt Number: {receiptNumber}\n{updatedNotes}";
                        }
                    }
                }
                else if (!string.IsNullOrEmpty(existingReceiptInfo) && animal.Sale != null)
                {
                    // Keep existing receipt info
                    if (!updatedNotes.Contains(existingReceiptInfo))
                    {
                        updatedNotes = $"{existingReceiptInfo}\n{updatedNotes}";
                    }
                }

                if (animal.Sale == null)
                {
                    // Create new sale record
                    var sale = new AnimalSale
                    {
                        AnimalId = animal.Id,
                        SaleDate = model.SaleDate,
                        SaleType = model.SaleType,
                        SalePrice = model.SalePrice,
                        WeightAtSale = model.WeightAtSale,
                        PricePerKg = model.WeightAtSale.HasValue ? 
                            Math.Round(model.SalePrice / (decimal)model.WeightAtSale.Value, 2) : null,
                        BuyerName = model.BuyerName,
                        InvoiceNumber = model.InvoiceNumber ?? $"INV{DateTime.Now:yyyyMMdd}-{await GetNextInvoiceNumber():D4}",
                        PaymentReceived = model.PaymentReceived,
                        PaymentDate = model.PaymentReceived ? (model.PaymentDate ?? DateTime.Now) : null,
                        Notes = updatedNotes
                    };

                    // Use execution strategy for database operations
                    var strategy = _context.Database.CreateExecutionStrategy();
                    await strategy.ExecuteAsync(async () => 
                    {
                        using (var transaction = await _context.Database.BeginTransactionAsync())
                        {
                            try
                            {
                                _context.AnimalSales.Add(sale);
                                animal.Status = AnimalStatus.Sold;
                                _context.Animals.Update(animal);
                                
                                await _context.SaveChangesAsync();
                                
                                // Record financial transaction
                                var financialTransaction = new FinancialTransaction
                                {
                                    TransactionType = "Income",
                                    Category = model.SaleType.ToString(),
                                    Amount = model.SalePrice,
                                    Description = $"Sale of {animal.TagNumber} - {animal.Type}",
                                    ReferenceNumber = sale.InvoiceNumber,
                                    TransactionDate = model.SaleDate,
                                    CreatedDate = DateTime.Now,
                                    Notes = string.Empty,
                                    PaymentMethod = "N/A",
                                    RecordedBy = User.Identity?.Name ?? "System",
                                    IsReconciled = false,
                                    RelatedEntityId = sale.Id,
                                    RelatedEntityType = "AnimalSale"
                                };
                                
                                _context.FinancialTransactions.Add(financialTransaction);
                                await _context.SaveChangesAsync();
                                
                                await transaction.CommitAsync();
                            }
                            catch
                            {
                                await transaction.RollbackAsync();
                                throw;
                            }
                        }
                    });
                }
                else
                {
                    // Calculate if there was a price change
                    bool priceChanged = animal.Sale.SalePrice != model.SalePrice;
                    decimal oldPrice = animal.Sale.SalePrice;
                    
                    // Update existing sale record
                    animal.Sale.SaleDate = model.SaleDate;
                    animal.Sale.SaleType = model.SaleType;
                    animal.Sale.SalePrice = model.SalePrice;
                    animal.Sale.WeightAtSale = model.WeightAtSale;
                    animal.Sale.PricePerKg = model.WeightAtSale.HasValue ? 
                        Math.Round(model.SalePrice / (decimal)model.WeightAtSale.Value, 2) : null;
                    animal.Sale.BuyerName = model.BuyerName;
                    animal.Sale.InvoiceNumber = model.InvoiceNumber;
                    animal.Sale.PaymentReceived = model.PaymentReceived;
                    animal.Sale.PaymentDate = model.PaymentReceived ? (model.PaymentDate ?? DateTime.Now) : null;
                    animal.Sale.Notes = updatedNotes;

                    // Use execution strategy for database operations
                    var strategy = _context.Database.CreateExecutionStrategy();
                    await strategy.ExecuteAsync(async () => 
                    {
                        using (var transaction = await _context.Database.BeginTransactionAsync())
                        {
                            try
                            {
                                _context.AnimalSales.Update(animal.Sale);
                                _context.Animals.Update(animal);
                                
                                await _context.SaveChangesAsync();
                                
                                // If price changed, record an adjustment transaction
                                if (priceChanged)
                                {
                                    decimal adjustmentAmount = model.SalePrice - oldPrice;
                                    var adjustmentTransaction = new FinancialTransaction
                                    {
                                        TransactionType = adjustmentAmount >= 0 ? "Income" : "Expense",
                                        Category = "Sale Adjustment",
                                        Amount = Math.Abs(adjustmentAmount),
                                        Description = $"Adjustment to sale price for {animal.TagNumber} - {animal.Type}",
                                        ReferenceNumber = animal.Sale.InvoiceNumber,
                                        TransactionDate = DateTime.Now,
                                        CreatedDate = DateTime.Now,
                                        Notes = string.Empty,
                                        PaymentMethod = "N/A",
                                        RecordedBy = User.Identity?.Name ?? "System",
                                        IsReconciled = false,
                                        RelatedEntityId = animal.Sale.Id,
                                        RelatedEntityType = "AnimalSale"
                                    };
                                    
                                    _context.FinancialTransactions.Add(adjustmentTransaction);
                                    await _context.SaveChangesAsync();
                                }
                                
                                await transaction.CommitAsync();
                            }
                            catch
                            {
                                await transaction.RollbackAsync();
                                throw;
                            }
                        }
                    });
                }

                TempData["SuccessMessage"] = $"Sale for animal {animal.TagNumber} successfully updated.";
                return RedirectToAction(nameof(AnimalSales));
            }

            await PopulateAnimalDropdowns();
            return View("RecordAnimalSale", model);
        }

        private async Task<int> GetNextInvoiceNumber()
        {
            var lastInvoice = await _context.AnimalSales
                .OrderByDescending(s => s.Id)
                .FirstOrDefaultAsync();
            
            if (lastInvoice == null)
                return 1;

            try
            {
                var lastNumber = int.Parse(lastInvoice.InvoiceNumber.Substring(lastInvoice.InvoiceNumber.LastIndexOf('-') + 1));
                return lastNumber + 1;
            }
            catch
            {
                return 1;
            }
        }

        // Helper method to record financial transactions
        private void RecordFinancialTransaction(string transactionType, string category, decimal amount, 
            string description, string referenceNumber, DateTime transactionDate)
        {
            // Check if FinancialTransaction model exists in the system
            // If not, this will be a no-op (we'll implement the full model below)
            try
            {
                var transaction = new FinancialTransaction
                {
                    TransactionType = transactionType,
                    Category = category,
                    Amount = amount,
                    Description = description ?? "No description provided", // Ensure not null
                    ReferenceNumber = referenceNumber ?? "None", // Ensure not null
                    TransactionDate = transactionDate,
                    CreatedDate = DateTime.Now,
                    Notes = string.Empty, // Set an empty string instead of null
                    PaymentMethod = "N/A", // Default value for required fields
                    RecordedBy = User.Identity?.Name ?? "System", // Default value if not authenticated
                    IsReconciled = false, // Default value
                    RelatedEntityType = "Manual" // Default for manually created transactions
                };
                
                _context.FinancialTransactions.Add(transaction);
            }
            catch (Exception ex)
            {
                // The model or DbSet might not exist yet, we'll implement it in the next step
                Console.WriteLine($"FinancialTransaction recording failed: {ex.Message}");
            }
        }

        // GET: Admin/FinancialRecords/Transactions
        public async Task<IActionResult> Transactions(DateTime? startDate, DateTime? endDate, string filterType, string filterCategory, string searchTerm)
        {
            // Set default date range if not provided
            var today = DateTime.Now.Date;
            startDate ??= new DateTime(today.Year, today.Month, 1);
            endDate ??= today;
            
            // Query transactions with filters
            var query = _context.FinancialTransactions.AsQueryable();
            
            // Apply date filter
            query = query.Where(t => t.TransactionDate >= startDate && t.TransactionDate <= endDate);
            
            // Apply type filter
            if (!string.IsNullOrWhiteSpace(filterType))
            {
                query = query.Where(t => t.TransactionType == filterType);
            }
            
            // Apply category filter
            if (!string.IsNullOrWhiteSpace(filterCategory))
            {
                query = query.Where(t => t.Category == filterCategory);
            }
            
            // Apply search term
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(t => 
                    t.Description.Contains(searchTerm) || 
                    t.ReferenceNumber.Contains(searchTerm) || 
                    t.Notes.Contains(searchTerm));
            }
            
            // Execute query and get results
            var transactions = await query.OrderByDescending(t => t.TransactionDate).ToListAsync();
            
            // Calculate totals for the summary section
            var totalIncome = transactions
                .Where(t => t.TransactionType == "Income")
                .Sum(t => t.Amount);
            
            var totalExpense = transactions
                .Where(t => t.TransactionType == "Expense")
                .Sum(t => t.Amount);
            
            // Get unique categories and transaction types for filters
            var categories = await _context.FinancialTransactions
                .Select(t => t.Category)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();
            
            var transactionTypes = await _context.FinancialTransactions
                .Select(t => t.TransactionType)
                .Distinct()
                .OrderBy(t => t)
                .ToListAsync();
            
            // Add default transaction types if none exist yet
            if (!transactionTypes.Any())
            {
                transactionTypes = new List<string> { "Income", "Expense", "Transfer", "Adjustment" };
            }
            
            // Set ViewData
            ViewData["StartDate"] = startDate;
            ViewData["EndDate"] = endDate;
            ViewData["FilterType"] = filterType;
            ViewData["FilterCategory"] = filterCategory;
            ViewData["SearchTerm"] = searchTerm;
            ViewData["Categories"] = categories;
            ViewData["TransactionTypes"] = transactionTypes;
            ViewData["TotalIncome"] = totalIncome;
            ViewData["TotalExpense"] = totalExpense;
            
            return View(transactions);
        }

        // POST: Admin/FinancialRecords/AddTransaction
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTransaction(FinancialTransaction transaction)
        {
            try
            {
                // Debug what's coming from the form
                Console.WriteLine($"AddTransaction called with: {transaction?.TransactionType ?? "null"}, {transaction?.Category ?? "null"}, {transaction?.Amount}");
                
                if (!ModelState.IsValid)
                {
                    // Collect the errors for debugging
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    
                    Console.WriteLine($"ModelState errors: {string.Join(", ", errors)}");
                    TempData["ErrorMessage"] = $"Please correct the following errors: {string.Join(", ", errors)}";
                    return RedirectToAction(nameof(Transactions));
                }
                
                // Set default values for all required fields to prevent NULL errors
                transaction.CreatedDate = DateTime.Now;
                transaction.RecordedBy = User.Identity?.Name ?? "System";
                
                // Ensure strings aren't null
                transaction.Description = transaction.Description ?? "No description";
                transaction.Notes = transaction.Notes ?? "";
                transaction.ReferenceNumber = transaction.ReferenceNumber ?? "None";
                transaction.PaymentMethod = transaction.PaymentMethod ?? "N/A";
                
                // Ensure RelatedEntityType is set
                if (string.IsNullOrEmpty(transaction.RelatedEntityType))
                {
                    transaction.RelatedEntityType = "Manual";
                }
                
                try
                {
                    _context.FinancialTransactions.Add(transaction);
                    await _context.SaveChangesAsync();
                    
                    TempData["SuccessMessage"] = "Transaction recorded successfully.";
                    Console.WriteLine($"Transaction saved successfully: ID = {transaction.Id}");
                    return RedirectToAction(nameof(Transactions));
                }
                catch (DbUpdateException dbEx)
                {
                    Console.WriteLine($"Database error saving transaction: {dbEx.Message}");
                    Console.WriteLine($"Inner exception: {dbEx.InnerException?.Message}");
                    TempData["ErrorMessage"] = $"Database error: {dbEx.InnerException?.Message ?? dbEx.Message}";
                    return RedirectToAction(nameof(Transactions));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error in AddTransaction: {ex.Message}");
                TempData["ErrorMessage"] = $"An unexpected error occurred: {ex.Message}";
                return RedirectToAction(nameof(Transactions));
            }
        }

        // POST: Admin/FinancialRecords/EditTransaction
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditTransaction(FinancialTransaction transaction)
        {
            try
            {
                // Debug what's coming from the form
                Console.WriteLine($"EditTransaction called with ID: {transaction.Id}, Type: {transaction?.TransactionType ?? "null"}");
                
                if (!ModelState.IsValid)
                {
                    // Collect the errors for debugging
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    
                    Console.WriteLine($"ModelState errors: {string.Join(", ", errors)}");
                    TempData["ErrorMessage"] = $"Please correct the following errors: {string.Join(", ", errors)}";
                    return RedirectToAction(nameof(Transactions));
                }
                
                var existingTransaction = await _context.FinancialTransactions.FindAsync(transaction.Id);
                
                if (existingTransaction == null)
                {
                    TempData["ErrorMessage"] = "Transaction not found.";
                    return RedirectToAction(nameof(Transactions));
                }
                
                // Update fields with null checks
                existingTransaction.TransactionType = transaction.TransactionType;
                existingTransaction.Category = transaction.Category;
                existingTransaction.Amount = transaction.Amount;
                existingTransaction.Description = transaction.Description ?? existingTransaction.Description ?? "No description";
                existingTransaction.ReferenceNumber = transaction.ReferenceNumber ?? existingTransaction.ReferenceNumber ?? "None";
                existingTransaction.TransactionDate = transaction.TransactionDate;
                existingTransaction.PaymentMethod = transaction.PaymentMethod ?? existingTransaction.PaymentMethod ?? "N/A";
                existingTransaction.IsReconciled = transaction.IsReconciled;
                existingTransaction.Notes = transaction.Notes ?? existingTransaction.Notes ?? "";
                
                // Make sure RelatedEntityType is preserved
                if (string.IsNullOrEmpty(transaction.RelatedEntityType))
                {
                    // Keep existing value or set default
                    if (string.IsNullOrEmpty(existingTransaction.RelatedEntityType))
                    {
                        existingTransaction.RelatedEntityType = "Manual";
                    }
                }
                else
                {
                    existingTransaction.RelatedEntityType = transaction.RelatedEntityType;
                }
                
                try
                {
                    _context.Update(existingTransaction);
                    await _context.SaveChangesAsync();
                    
                    TempData["SuccessMessage"] = "Transaction updated successfully.";
                    return RedirectToAction(nameof(Transactions));
                }
                catch (DbUpdateException dbEx)
                {
                    Console.WriteLine($"Database error updating transaction: {dbEx.Message}");
                    Console.WriteLine($"Inner exception: {dbEx.InnerException?.Message}");
                    TempData["ErrorMessage"] = $"Database error: {dbEx.InnerException?.Message ?? dbEx.Message}";
                    return RedirectToAction(nameof(Transactions));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error in EditTransaction: {ex.Message}");
                TempData["ErrorMessage"] = $"An unexpected error occurred: {ex.Message}";
                return RedirectToAction(nameof(Transactions));
            }
        }

        // POST: Admin/FinancialRecords/DeleteTransaction
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTransaction(int id)
        {
            try
            {
                Console.WriteLine($"DeleteTransaction called with ID: {id}");
                
                var transaction = await _context.FinancialTransactions.FindAsync(id);
                
                if (transaction == null)
                {
                    Console.WriteLine($"Transaction with ID {id} not found");
                    TempData["ErrorMessage"] = "Transaction not found.";
                    return RedirectToAction(nameof(Transactions));
                }
                
                _context.FinancialTransactions.Remove(transaction);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Transaction deleted successfully.";
                return RedirectToAction(nameof(Transactions));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting transaction: {ex.Message}");
                TempData["ErrorMessage"] = $"Error deleting transaction: {ex.Message}";
                return RedirectToAction(nameof(Transactions));
            }
        }

        // GET: Admin/FinancialRecords/GetTransaction
        [HttpGet]
        public async Task<IActionResult> GetTransaction(int id)
        {
            try
            {
                Console.WriteLine($"GetTransaction called with ID: {id}");
                
                var transaction = await _context.FinancialTransactions.FindAsync(id);
                
                if (transaction == null)
                {
                    Console.WriteLine($"Transaction with ID {id} not found");
                    return NotFound(new { error = "Transaction not found" });
                }
                
                return Json(transaction);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving transaction: {ex.Message}");
                return StatusCode(500, new { error = $"Internal server error: {ex.Message}" });
            }
        }

        // GET: Admin/FinancialRecords/ExportTransactions
        public async Task<IActionResult> ExportTransactions(DateTime? startDate, DateTime? endDate, string filterType, string filterCategory, string searchTerm)
        {
            // Set default date range if not provided
            var today = DateTime.Now.Date;
            startDate ??= new DateTime(today.Year, today.Month, 1);
            endDate ??= today;
            
            // Query transactions with filters (same as Transactions action)
            var query = _context.FinancialTransactions.AsQueryable();
            
            // Apply date filter
            query = query.Where(t => t.TransactionDate >= startDate && t.TransactionDate <= endDate);
            
            // Apply type filter
            if (!string.IsNullOrWhiteSpace(filterType))
            {
                query = query.Where(t => t.TransactionType == filterType);
            }
            
            // Apply category filter
            if (!string.IsNullOrWhiteSpace(filterCategory))
            {
                query = query.Where(t => t.Category == filterCategory);
            }
            
            // Apply search term
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(t => 
                    t.Description.Contains(searchTerm) || 
                    t.ReferenceNumber.Contains(searchTerm) || 
                    t.Notes.Contains(searchTerm));
            }
            
            // Execute query and get results
            var transactions = await query.OrderByDescending(t => t.TransactionDate).ToListAsync();
            
            // Build CSV content
            var csv = new System.Text.StringBuilder();
            
            // Add headers
            csv.AppendLine("Date,Type,Category,Description,Reference,Amount,Payment Method,Reconciled,Notes");
            
            // Add data
            foreach (var t in transactions)
            {
                var date = t.TransactionDate.ToString("yyyy-MM-dd");
                var type = t.TransactionType;
                var category = t.Category;
                var description = $"\"{t.Description?.Replace("\"", "\"\"")}\""; // Handle quotes in CSV
                var reference = $"\"{t.ReferenceNumber?.Replace("\"", "\"\"")}\"";
                var amount = t.Amount.ToString("F2");
                var paymentMethod = t.PaymentMethod ?? "";
                var reconciled = t.IsReconciled ? "Yes" : "No";
                var notes = $"\"{t.Notes?.Replace("\"", "\"\"")}\"";
                
                csv.AppendLine($"{date},{type},{category},{description},{reference},{amount},{paymentMethod},{reconciled},{notes}");
            }
            
            // Set file name
            var fileName = $"Financial_Transactions_{startDate:yyyy-MM-dd}_to_{endDate:yyyy-MM-dd}.csv";
            
            // Return as file
            return File(System.Text.Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", fileName);
        }

        // GET: Admin/FinancialRecords/TestRecordAnimalSale
        public IActionResult TestRecordAnimalSale()
        {
            try
            {
                // Create a basic model
                var model = new Inyama_Yethu.Models.ViewModels.AnimalSaleViewModel
                {
                    SaleDate = DateTime.Today,
                    PaymentReceived = false
                };

                // Create an empty animals list
                ViewBag.Animals = new SelectList(new List<SelectListItem>());
                
                // Add a model error for testing
                ModelState.AddModelError("", "This is a test error message");
                
                // Return the view with the model
                return View("RecordAnimalSale", model);
            }
            catch (Exception ex)
            {
                // Return a view with the error details
                return Content($"Error: {ex.Message}\nStack Trace: {ex.StackTrace}");
            }
        }

        // GET: Admin/FinancialRecords/DiagnoseSalesIssues
        public async Task<IActionResult> DiagnoseSalesIssues()
        {
            var report = new Dictionary<string, string>();
            
            try
            {
                // 1. Check if Animals table exists and has records
                var animalCount = await _context.Animals.CountAsync();
                report["Animals Table"] = $"Found {animalCount} total animals";
                
                // 2. Check if any animals are active
                var activeAnimalCount = await _context.Animals
                    .Where(a => a.Status == AnimalStatus.Active)
                    .CountAsync();
                report["Active Animals"] = $"Found {activeAnimalCount} active animals";
                
                // 3. Check if AnimalSales table exists
                bool salesTableExists = true;
                try
                {
                    var salesCount = await _context.AnimalSales.CountAsync();
                    report["Animal Sales Table"] = $"Found {salesCount} existing sales";
                }
                catch (Exception ex)
                {
                    salesTableExists = false;
                    report["Animal Sales Table"] = $"Error: {ex.Message}";
                }
                
                // 4. Check if FinancialTransactions table exists
                bool financialTableExists = true;
                try
                {
                    var transactionCount = await _context.FinancialTransactions.CountAsync();
                    report["Financial Transactions Table"] = $"Found {transactionCount} existing transactions";
                }
                catch (Exception ex)
                {
                    financialTableExists = false;
                    report["Financial Transactions Table"] = $"Error: {ex.Message}";
                }
                
                // 5. Verify database connection
                report["Database Connection"] = "Active";
                
                // 6. Check if all necessary enum values exist
                var saleTypeValues = Enum.GetNames(typeof(SaleType)).ToList();
                report["Sale Types"] = string.Join(", ", saleTypeValues);
                
                // 7. Check if AnimalStatus enum has Sold value
                var statusValues = Enum.GetNames(typeof(AnimalStatus)).ToList();
                report["Animal Status Values"] = string.Join(", ", statusValues);
                
                // 8. Overall assessment
                if (activeAnimalCount == 0)
                {
                    report["DIAGNOSIS"] = "NO ACTIVE ANIMALS: You need to add animals with 'Active' status before you can record sales.";
                }
                else if (!salesTableExists)
                {
                    report["DIAGNOSIS"] = "MISSING SALES TABLE: The AnimalSales table does not exist or is inaccessible.";
                }
                else if (!financialTableExists)
                {
                    report["DIAGNOSIS"] = "MISSING FINANCIAL TABLE: The system will now try to create the Financial Transactions table.";
                    await EnsureFinancialTransactionsExists();
                }
                else
                {
                    report["DIAGNOSIS"] = "All systems appear to be functioning correctly. If you're still having issues, check the error logs for more details.";
                }
            }
            catch (Exception ex)
            {
                report["ERROR"] = $"Exception during diagnosis: {ex.Message}";
            }
            
            return Json(report);
        }
    }
} 