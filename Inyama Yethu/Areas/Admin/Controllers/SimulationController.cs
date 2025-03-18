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
    public class SimulationController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SimulationController> _logger;

        public SimulationController(ApplicationDbContext context, ILogger<SimulationController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Admin/Simulation
        public async Task<IActionResult> Index()
        {
            var animals = await _context.Animals.ToListAsync();
            return View(animals);
        }

        // GET: Admin/Simulation/PigDetail/5
        public async Task<IActionResult> PigDetail(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var animal = await _context.Animals
                .Include(a => a.Mother)
                .Include(a => a.Father)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (animal == null)
            {
                return NotFound();
            }

            return View(animal);
        }

        // GET: Admin/Simulation/RunSimulation
        public IActionResult RunSimulation()
        {
            // This will be used to run various simulation scenarios
            return View();
        }

        // GET: Admin/Simulation/SeedAnimals
        public async Task<IActionResult> SeedAnimals()
        {
            try
            {
                // Check if animals already exist
                bool anyAnimals = await _context.Animals.AnyAsync();
                
                if (!anyAnimals)
                {
                    _logger.LogInformation("Starting to seed animals into database");
                    
                    var random = new Random();
                    var now = DateTime.Now;
                    
                    // Step 1: Create and save base breeding stock (boars and sows)
                    try
                    {
                        _logger.LogInformation("Step 1: Creating breeding stock (boars and sows)");
                        
                        // Create 2 boars
                        var boars = new List<Animal>();
                        for (int i = 1; i <= 2; i++)
                        {
                            // Age the boars between 12-24 months
                            var boarBirthDate = now.AddDays(-random.Next(365, 730));
                            
                            var boar = new Animal
                            {
                                TagNumber = $"B{i.ToString("D3")}",
                                Type = AnimalType.Boar,
                                Gender = Gender.Male,
                                BirthDate = boarBirthDate,
                                Status = AnimalStatus.Active,
                                Weight = 180 + (random.NextDouble() * 40), // 180-220 kg
                                Notes = $"Seeded boar #{i}"
                            };
                            
                            _context.Animals.Add(boar);
                            boars.Add(boar);
                        }
                        
                        // Create 8 sows
                        var sows = new List<Animal>();
                        for (int i = 1; i <= 8; i++)
                        {
                            // Age the sows between 10-36 months
                            var sowBirthDate = now.AddDays(-random.Next(300, 1095));
                            
                            var sow = new Animal
                            {
                                TagNumber = $"S{i.ToString("D3")}",
                                Type = AnimalType.Sow,
                                Gender = Gender.Female,
                                BirthDate = sowBirthDate,
                                Status = AnimalStatus.Active,
                                Weight = 160 + (random.NextDouble() * 60), // 160-220 kg
                                Notes = $"Seeded sow #{i}"
                            };
                            
                            _context.Animals.Add(sow);
                            sows.Add(sow);
                        }
                        
                        // Save to generate IDs for the breeding stock
                        await _context.SaveChangesAsync();
                        _logger.LogInformation("Successfully created and saved breeding stock");
                        
                        // Step 2: Create and save weaners and growers (without parent relationships for simplicity)
                        _logger.LogInformation("Step 2: Creating weaners and growers");
                        
                        // Create some weaners
                        int numWeaners = random.Next(5, 10);
                        for (int i = 1; i <= numWeaners; i++)
                        {
                            // Weaners are 4-10 weeks old
                            var weanerBirthDate = now.AddDays(-random.Next(28, 70));
                            
                            var weaner = new Animal
                            {
                                TagNumber = $"W{i.ToString("D3")}",
                                Type = AnimalType.Weaner,
                                Gender = random.Next(2) == 0 ? Gender.Male : Gender.Female,
                                BirthDate = weanerBirthDate,
                                Status = AnimalStatus.Active,
                                Weight = 7.0 + (random.NextDouble() * 8.0), // 7-15 kg
                                Notes = $"Weaner #{i}"
                                // We'll skip parent relationships for now to simplify
                            };
                            
                            _context.Animals.Add(weaner);
                        }
                        
                        // Create some growers 
                        int numGrowers = random.Next(3, 7);
                        for (int i = 1; i <= numGrowers; i++)
                        {
                            // Growers are 10-20 weeks old
                            var growerBirthDate = now.AddDays(-random.Next(70, 140));
                            
                            var grower = new Animal
                            {
                                TagNumber = $"G{i.ToString("D3")}",
                                Type = AnimalType.Grower,
                                Gender = random.Next(2) == 0 ? Gender.Male : Gender.Female,
                                BirthDate = growerBirthDate,
                                Status = AnimalStatus.Active,
                                Weight = 30.0 + (random.NextDouble() * 30.0), // 30-60 kg
                                Notes = $"Grower #{i}"
                            };
                            
                            _context.Animals.Add(grower);
                        }
                        
                        // Save weaners and growers
                        await _context.SaveChangesAsync();
                        _logger.LogInformation("Successfully created and saved weaners and growers");
                        
                        // Step 3: Create matings and piglets for selected sows
                        _logger.LogInformation("Step 3: Creating matings and piglets");
                        
                        try {
                            // Create piglets for just 2 sows to keep it simple
                            int sowsWithPiglets = Math.Min(2, sows.Count);
                            
                            for (int i = 0; i < sowsWithPiglets; i++)
                            {
                                // Select a sow to be the mother
                                var mother = sows[i];
                                
                                // Select a random boar to be the father
                                var father = boars[random.Next(boars.Count)];
                                
                                // Create a completed mating record (this happened about 4 months ago)
                                var matingDate = now.AddDays(-random.Next(115, 130)); // A bit longer than gestation to account for piglets being a couple weeks old
                                var farrowingDate = matingDate.AddDays(114); // Normal pig gestation is 114 days
                                
                                var mating = new Mating
                                {
                                    MotherAnimalId = mother.Id,
                                    FatherAnimalId = father.Id,
                                    MatingDate = matingDate,
                                    ExpectedFarrowingDate = matingDate.AddDays(114),
                                    ActualFarrowingDate = farrowingDate,
                                    PregnancyCheck1Result = true,
                                    Status = MatingStatus.Farrowed,
                                    Notes = "Seeded mating record",
                                    NumberOfPigletsBorn = random.Next(8, 14),
                                    NumberOfPigletsBornAlive = random.Next(7, 12),
                                    Vaccination1Completed = true,
                                    Vaccination2Completed = true
                                };
                                
                                _context.Matings.Add(mating);
                                
                                // Update mother status
                                mother.Status = AnimalStatus.Farrowing;
                                
                                // Save to generate mating ID
                                await _context.SaveChangesAsync();
                                
                                // Create just a few piglets for this mother (3-5)
                                int numPiglets = random.Next(3, 6);
                                
                                for (int j = 1; j <= numPiglets; j++)
                                {
                                    try
                                    {
                                        // Age the piglets between 1-28 days (young piglets)
                                        var pigletBirthDate = farrowingDate;
                                        
                                        var piglet = new Animal
                                        {
                                            TagNumber = $"P{pigletBirthDate:yyMMdd}-{j}",
                                            Type = AnimalType.Piglet,
                                            Gender = random.Next(2) == 0 ? Gender.Male : Gender.Female,
                                            BirthDate = pigletBirthDate,
                                            Status = AnimalStatus.Active,
                                            Weight = 1.0 + (random.NextDouble() * 1.5), // 1-2.5 kg
                                            Notes = $"Piglet #{j} born to Sow {mother.TagNumber}",
                                            MotherAnimalId = mother.Id,
                                            FatherAnimalId = father.Id
                                        };
                                        
                                        _context.Animals.Add(piglet);
                                        
                                        // Save each piglet individually to isolate any errors
                                        await _context.SaveChangesAsync();
                                        _logger.LogInformation($"Saved piglet #{j} for sow {mother.TagNumber}");
                                    }
                                    catch (Exception ex)
                                    {
                                        // Log but continue with next piglet
                                        _logger.LogError(ex, $"Error creating piglet #{j} for sow {mother.TagNumber}: {ex.Message}");
                                    }
                                }
                            }
                            
                            _logger.LogInformation("Successfully created and saved matings and piglets");
                        }
                        catch (Exception ex)
                        {
                            // Log but continue with basic animals
                            _logger.LogError(ex, $"Error creating matings and piglets: {ex.Message}");
                            TempData["Warning"] = "Created basic animals but encountered an error with matings and piglets.";
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log and rethrow to outer handler
                        _logger.LogError(ex, $"Error in base animal creation: {ex.Message}");
                        throw;
                    }
                    
                    TempData["Success"] = "Successfully seeded animal database with pigs at various life stages!";
                }
                else
                {
                    TempData["Info"] = "Animals already exist in the database.";
                }
                
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Get the innermost exception message for more details
                var innerException = ex;
                string detailedMessage = ex.Message;
                
                while (innerException.InnerException != null) 
                {
                    innerException = innerException.InnerException;
                    detailedMessage += " → " + innerException.Message;
                }
                
                _logger.LogError(ex, $"Error seeding animals: {detailedMessage}");
                TempData["Error"] = $"An error occurred while seeding animals: {detailedMessage}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Admin/Simulation/RunMatingSimulation
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RunMatingSimulation()
        {
            try
            {
                // Simple simulation of a mating between a boar and sow
                var boar = await _context.Animals
                    .FirstOrDefaultAsync(a => a.Type == AnimalType.Boar && a.Status == AnimalStatus.Active);
                
                var sow = await _context.Animals
                    .FirstOrDefaultAsync(a => a.Type == AnimalType.Sow && a.Status == AnimalStatus.Active);

                if (boar == null || sow == null)
                {
                    TempData["Error"] = "Cannot find available boar and sow for mating simulation";
                    return RedirectToAction(nameof(RunSimulation));
                }

                // Create a new mating record
                var mating = new Mating
                {
                    MotherAnimalId = sow.Id,
                    FatherAnimalId = boar.Id,
                    MatingDate = DateTime.Now,
                    Notes = "Simulated mating",
                    ExpectedFarrowingDate = DateTime.Now.AddDays(114), // 114 days gestation
                    Status = MatingStatus.Completed
                };

                // Update sow status
                sow.Status = AnimalStatus.Mating;

                _context.Matings.Add(mating);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Successfully simulated mating between Boar {boar.TagNumber} and Sow {sow.TagNumber}";
                
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during mating simulation");
                TempData["Error"] = "An error occurred during simulation: " + ex.Message;
                return RedirectToAction(nameof(RunSimulation));
            }
        }
        
        // POST: Admin/Simulation/RunPregnancyCheckSimulation
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RunPregnancyCheckSimulation()
        {
            try
            {
                // Find sows in mating status
                var matedSow = await _context.Animals
                    .Include(a => a.MatingsAsMother)
                    .FirstOrDefaultAsync(a => a.Type == AnimalType.Sow && a.Status == AnimalStatus.Mating);

                if (matedSow == null)
                {
                    TempData["Error"] = "Cannot find any sows in mating status for pregnancy check";
                    return RedirectToAction(nameof(RunSimulation));
                }

                // Get the latest mating record
                var latestMating = await _context.Matings
                    .Where(m => m.MotherAnimalId == matedSow.Id)
                    .OrderByDescending(m => m.MatingDate)
                    .FirstOrDefaultAsync();

                if (latestMating == null)
                {
                    TempData["Error"] = "Cannot find mating record for the sow";
                    return RedirectToAction(nameof(RunSimulation));
                }

                // Simulate pregnancy check (random result with 80% success rate)
                var random = new Random();
                bool isPregnant = random.NextDouble() < 0.8; // 80% chance of success

                // Update mating record with pregnancy check result
                latestMating.PregnancyCheck1Result = isPregnant;
                latestMating.Status = isPregnant ? MatingStatus.PregnancyConfirmed : MatingStatus.NotPregnant;
                
                // Update sow status
                matedSow.Status = isPregnant ? AnimalStatus.Pregnant : AnimalStatus.Active;

                await _context.SaveChangesAsync();

                if (isPregnant)
                {
                    TempData["Success"] = $"Pregnancy confirmed for Sow {matedSow.TagNumber}. Expected farrowing date: {latestMating.ExpectedFarrowingDate:MM/dd/yyyy}";
                }
                else
                {
                    TempData["Info"] = $"Pregnancy not confirmed for Sow {matedSow.TagNumber}. She has been returned to active status.";
                }
                
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during pregnancy check simulation");
                TempData["Error"] = "An error occurred during simulation: " + ex.Message;
                return RedirectToAction(nameof(RunSimulation));
            }
        }
        
        // POST: Admin/Simulation/RunFarrowingSimulation
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RunFarrowingSimulation()
        {
            try
            {
                // Find pregnant sow
                var pregnantSow = await _context.Animals
                    .FirstOrDefaultAsync(a => a.Type == AnimalType.Sow && a.Status == AnimalStatus.Pregnant);

                if (pregnantSow == null)
                {
                    TempData["Error"] = "Cannot find any pregnant sows for farrowing simulation";
                    return RedirectToAction(nameof(RunSimulation));
                }

                // Get the mating record
                var mating = await _context.Matings
                    .Where(m => m.MotherAnimalId == pregnantSow.Id && m.Status == MatingStatus.PregnancyConfirmed)
                    .OrderByDescending(m => m.MatingDate)
                    .FirstOrDefaultAsync();

                if (mating == null)
                {
                    TempData["Error"] = "Cannot find pregnancy record for the sow";
                    return RedirectToAction(nameof(RunSimulation));
                }

                // Simulate farrowing
                var random = new Random();
                int pigletsBorn = random.Next(8, 16); // 8-15 piglets born
                int pigletsAlive = Math.Max(pigletsBorn - random.Next(0, 3), 0); // Some piglets might not survive

                // Update mating record
                mating.Status = MatingStatus.Farrowed;
                mating.ActualFarrowingDate = DateTime.Now;
                mating.NumberOfPigletsBorn = pigletsBorn;
                mating.NumberOfPigletsBornAlive = pigletsAlive;

                // Create piglet records
                for (int i = 1; i <= pigletsAlive; i++)
                {
                    var piglet = new Animal
                    {
                        TagNumber = $"P{DateTime.Now:yyMMdd}-{i}",
                        Type = AnimalType.Piglet,
                        Gender = random.Next(0, 2) == 0 ? Gender.Male : Gender.Female,
                        BirthDate = DateTime.Now,
                        Status = AnimalStatus.Active,
                        Weight = 1.0 + (random.NextDouble() * 0.5), // 1.0-1.5 kg birth weight
                        Notes = "Born via simulated farrowing",
                        MotherAnimalId = pregnantSow.Id,
                        FatherAnimalId = mating.FatherAnimalId
                    };
                    _context.Animals.Add(piglet);
                }

                // Update sow status
                pregnantSow.Status = AnimalStatus.Farrowing;

                await _context.SaveChangesAsync();

                TempData["Success"] = $"Successful farrowing for Sow {pregnantSow.TagNumber}. {pigletsBorn} piglets born, {pigletsAlive} alive.";
                
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during farrowing simulation");
                TempData["Error"] = "An error occurred during simulation: " + ex.Message;
                return RedirectToAction(nameof(RunSimulation));
            }
        }
        
        // POST: Admin/Simulation/RunPigletProcessingSimulation
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RunPigletProcessingSimulation()
        {
            try
            {
                // Find piglets that haven't been processed
                var piglets = await _context.Animals
                    .Where(a => a.Type == AnimalType.Piglet)
                    .Take(5) // Process up to 5 piglets at a time
                    .ToListAsync();

                if (piglets.Count == 0)
                {
                    TempData["Error"] = "No piglets found for processing";
                    return RedirectToAction(nameof(RunSimulation));
                }

                // Check if there are any piglet processing records for these piglets
                var processedPigletIds = await _context.PigletProcessings
                    .Where(p => piglets.Select(pig => pig.Id).Contains(p.AnimalId))
                    .Select(p => p.AnimalId)
                    .ToListAsync();

                // Filter out already processed piglets
                var unprocessedPiglets = piglets.Where(p => !processedPigletIds.Contains(p.Id)).ToList();

                if (unprocessedPiglets.Count == 0)
                {
                    TempData["Info"] = "All available piglets have already been processed";
                    return RedirectToAction(nameof(RunSimulation));
                }

                // Process the piglets
                foreach (var piglet in unprocessedPiglets)
                {
                    var now = DateTime.Now;
                    var processing = new PigletProcessing
                    {
                        AnimalId = piglet.Id,
                        TailDockingCompleted = true,
                        TailDockingDate = now,
                        IronInjectionCompleted = true,
                        IronInjectionDate = now,
                        EarNotchingCompleted = true,
                        EarNotchingDate = now,
                        InitialVaccinationCompleted = true,
                        InitialVaccinationDate = now,
                        TattooingCompleted = true,
                        TattooingDate = now,
                        Notes = "Simulated piglet processing"
                    };
                    
                    _context.PigletProcessings.Add(processing);
                }

                await _context.SaveChangesAsync();

                TempData["Success"] = $"Successfully processed {unprocessedPiglets.Count} piglets.";
                
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during piglet processing simulation");
                TempData["Error"] = "An error occurred during simulation: " + ex.Message;
                return RedirectToAction(nameof(RunSimulation));
            }
        }
        
        // POST: Admin/Simulation/RunWeaningSimulation
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RunWeaningSimulation()
        {
            try
            {
                // Find piglets ready for weaning (older than 28 days)
                var piglets = await _context.Animals
                    .Where(a => a.Type == AnimalType.Piglet && DateTime.Now.Subtract(a.BirthDate).TotalDays >= 28)
                    .Take(10) // Wean up to 10 piglets at a time
                    .ToListAsync();

                if (piglets.Count == 0)
                {
                    TempData["Error"] = "No piglets old enough for weaning";
                    return RedirectToAction(nameof(RunSimulation));
                }

                // Update piglets to weaners
                foreach (var piglet in piglets)
                {
                    piglet.Type = AnimalType.Weaner;
                    
                    // Add a weight record
                    var random = new Random();
                    var weightRecord = new WeightRecord
                    {
                        AnimalId = piglet.Id,
                        RecordDate = DateTime.Now,
                        Weight = 7.0 + (random.NextDouble() * 3.0), // 7-10 kg weaning weight
                        Notes = "Weaning weight"
                    };
                    
                    _context.WeightRecords.Add(weightRecord);
                    
                    // Update the animal's current weight
                    piglet.Weight = weightRecord.Weight;
                }

                // If there are sows in farrowing status with no more piglets, update their status
                var pigletsByMother = await _context.Animals
                    .Where(a => a.Type == AnimalType.Piglet)
                    .GroupBy(a => a.MotherAnimalId)
                    .Select(g => new { MotherAnimalId = g.Key, PigletCount = g.Count() })
                    .ToListAsync();
                
                var sowsInFarrowing = await _context.Animals
                    .Where(a => a.Type == AnimalType.Sow && a.Status == AnimalStatus.Farrowing)
                    .ToListAsync();
                
                foreach (var sow in sowsInFarrowing)
                {
                    var motherInfo = pigletsByMother.FirstOrDefault(p => p.MotherAnimalId == sow.Id);
                    if (motherInfo == null || motherInfo.PigletCount == 0)
                    {
                        sow.Status = AnimalStatus.Active; // Return to active status
                    }
                }

                await _context.SaveChangesAsync();

                TempData["Success"] = $"Successfully weaned {piglets.Count} piglets into weaners.";
                
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during weaning simulation");
                TempData["Error"] = "An error occurred during simulation: " + ex.Message;
                return RedirectToAction(nameof(RunSimulation));
            }
        }
        
        // POST: Admin/Simulation/RunAbattoirShipmentSimulation
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RunAbattoirShipmentSimulation()
        {
            try
            {
                // Find finishers ready for shipment (finishers or growers)
                var finishers = await _context.Animals
                    .Where(a => a.Type == AnimalType.Finisher || a.Type == AnimalType.Grower)
                    .Take(10) // Ship up to 10 pigs
                    .ToListAsync();
                
                // If not enough finishers/growers, use weaners
                if (finishers.Count < 10)
                {
                    var weaners = await _context.Animals
                        .Where(a => a.Type == AnimalType.Weaner)
                        .Take(10 - finishers.Count)
                        .ToListAsync();
                    
                    finishers.AddRange(weaners);
                }

                // If still not enough, use any pig type except boars and sows
                if (finishers.Count < 10)
                {
                    var otherPigs = await _context.Animals
                        .Where(a => a.Type != AnimalType.Boar && a.Type != AnimalType.Sow && !finishers.Select(f => f.Id).Contains(a.Id))
                        .Take(10 - finishers.Count)
                        .ToListAsync();
                    
                    finishers.AddRange(otherPigs);
                }

                if (finishers.Count == 0)
                {
                    TempData["Error"] = "No pigs available for shipment to abattoir";
                    return RedirectToAction(nameof(RunSimulation));
                }

                // Create a new shipment record
                var random = new Random();
                var totalWeight = finishers.Sum(f => f.Weight ?? 0);
                var pricePerKg = 25.0 + (random.NextDouble() * 10.0); // R25-R35 per kg
                var totalValue = totalWeight * pricePerKg;
                var transportCost = 200 + (random.NextDouble() * 100); // R200-R300 transport cost
                
                var shipment = new AbattoirShipment
                {
                    ShipmentDate = DateTime.Now,
                    AbattoirName = "Local Abattoir",
                    NumberOfPigs = finishers.Count,
                    TotalWeight = totalWeight,
                    EstimatedValue = (decimal)totalValue,
                    TransportationCost = (decimal)transportCost,
                    ActualPayment = (decimal)(totalValue - transportCost),
                    Notes = "Simulated abattoir shipment"
                };
                
                _context.AbattoirShipments.Add(shipment);
                
                // Update status of shipped animals
                foreach (var animal in finishers)
                {
                    animal.Status = AnimalStatus.Sold;
                }

                await _context.SaveChangesAsync();

                TempData["Success"] = $"Successfully simulated shipment of {finishers.Count} pigs to the abattoir. " +
                                     $"Total weight: {totalWeight:F1} kg, Value: R{totalValue:F2}";
                
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during abattoir shipment simulation");
                TempData["Error"] = "An error occurred during simulation: " + ex.Message;
                return RedirectToAction(nameof(RunSimulation));
            }
        }
        
        // POST: Admin/Simulation/RunGrowerProgressionSimulation
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RunGrowerProgressionSimulation()
        {
            try
            {
                // Progress weaners to growers if they're old enough
                var weanersToProgress = await _context.Animals
                    .Where(a => a.Type == AnimalType.Weaner && DateTime.Now.Subtract(a.BirthDate).TotalDays > 70)
                    .ToListAsync();
                
                foreach (var weaner in weanersToProgress)
                {
                    weaner.Type = AnimalType.Grower;
                    
                    // Add a weight record
                    var random = new Random();
                    var weightRecord = new WeightRecord
                    {
                        AnimalId = weaner.Id,
                        RecordDate = DateTime.Now,
                        Weight = 30.0 + (random.NextDouble() * 10.0), // 30-40 kg
                        Notes = "Transition to grower"
                    };
                    
                    _context.WeightRecords.Add(weightRecord);
                    weaner.Weight = weightRecord.Weight;
                }

                // Progress growers to finishers if they're old enough
                var growersToProgress = await _context.Animals
                    .Where(a => a.Type == AnimalType.Grower && DateTime.Now.Subtract(a.BirthDate).TotalDays > 140)
                    .ToListAsync();
                
                foreach (var grower in growersToProgress)
                {
                    grower.Type = AnimalType.Finisher;
                    
                    // Add a weight record
                    var random = new Random();
                    var weightRecord = new WeightRecord
                    {
                        AnimalId = grower.Id,
                        RecordDate = DateTime.Now,
                        Weight = 80.0 + (random.NextDouble() * 40.0), // 80-120 kg
                        Notes = "Transition to finisher"
                    };
                    
                    _context.WeightRecords.Add(weightRecord);
                    grower.Weight = weightRecord.Weight;
                }

                // If no pigs were processed, add some feeding records instead
                if (weanersToProgress.Count == 0 && growersToProgress.Count == 0)
                {
                    // Add feeding records for all growing pigs
                    var growingPigs = await _context.Animals
                        .Where(a => a.Type == AnimalType.Piglet || a.Type == AnimalType.Weaner || a.Type == AnimalType.Grower)
                        .ToListAsync();
                    
                    if (growingPigs.Any())
                    {
                        var random = new Random();
                        
                        foreach (var pig in growingPigs)
                        {
                            // Determine feed amount based on type
                            double feedAmount = 0;
                            decimal costPerKg = 0;
                            FeedType feedType = FeedType.Other;
                            
                            switch (pig.Type)
                            {
                                case AnimalType.Piglet:
                                    feedAmount = 0.3 + (random.NextDouble() * 0.3); // 0.3-0.6 kg
                                    costPerKg = 12.50m; // R12.50 per kg
                                    feedType = FeedType.CreepPellets;
                                    break;
                                case AnimalType.Weaner:
                                    feedAmount = 1.0 + (random.NextDouble() * 1.0); // 1-2 kg
                                    costPerKg = 8.75m; // R8.75 per kg
                                    feedType = FeedType.WeanerFeed;
                                    break;
                                case AnimalType.Grower:
                                    feedAmount = 2.5 + (random.NextDouble() * 1.5); // 2.5-4 kg
                                    costPerKg = 7.25m; // R7.25 per kg
                                    feedType = FeedType.GrowerFeed;
                                    break;
                            }
                            
                            var feeding = new Feeding
                            {
                                AnimalId = pig.Id,
                                FeedDate = DateTime.Now,
                                FeedType = feedType,
                                Quantity = feedAmount,
                                CostPerKg = costPerKg,
                                Notes = "Simulated daily feeding"
                            };
                            
                            _context.Feedings.Add(feeding);
                        }
                    }
                    else
                    {
                        TempData["Info"] = "No pigs available for progression or feeding";
                        return RedirectToAction(nameof(RunSimulation));
                    }
                }

                await _context.SaveChangesAsync();

                if (weanersToProgress.Count > 0 || growersToProgress.Count > 0)
                {
                    TempData["Success"] = $"Successfully progressed {weanersToProgress.Count} weaners to growers and {growersToProgress.Count} growers to finishers.";
                }
                else
                {
                    TempData["Success"] = "Recorded daily feeding for all growing pigs.";
                }
                
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during progression simulation");
                TempData["Error"] = "An error occurred during simulation: " + ex.Message;
                return RedirectToAction(nameof(RunSimulation));
            }
        }
    }
} 