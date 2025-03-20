using Inyama_Yethu.Data;
using Inyama_Yethu.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Inyama_Yethu.Services
{
    public class TaskSchedulerService : BackgroundService
    {
        private readonly ILogger<TaskSchedulerService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly TimeSpan _checkInterval = TimeSpan.FromHours(12); // Check twice daily

        public TaskSchedulerService(
            ILogger<TaskSchedulerService> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Task Scheduler Service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Task Scheduler is checking for upcoming tasks at: {time}", DateTimeOffset.Now);

                try
                {
                    await ProcessScheduledTasks(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while processing scheduled tasks.");
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }
        }

        private async Task ProcessScheduledTasks(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Get current date in South African context
            var today = DateTime.Now;
            var tomorrow = today.AddDays(1);

            await CreateMatingScheduleTasks(dbContext, today, stoppingToken);
            await CreatePregnancyCheckTasks(dbContext, today, tomorrow, stoppingToken);
            await CreateVaccinationTasks(dbContext, today, tomorrow, stoppingToken);
            await CreatePigletProcessingTasks(dbContext, today, stoppingToken);
            await CreateWeaningTasks(dbContext, today, stoppingToken);
            await CreateAbattoirScheduleTasks(dbContext, today, stoppingToken);
        }

        private async Task CreateMatingScheduleTasks(ApplicationDbContext dbContext, DateTime today, CancellationToken stoppingToken)
        {
            // Schedule 2 sows for mating each week (as per requirements)
            // Find eligible sows that aren't pregnant or already scheduled
            var eligibleSows = await dbContext.Animals
                .Where(a => a.Type == AnimalType.Sow && 
                           a.Status == AnimalStatus.Active && 
                           !a.MatingsAsMother.Any(m => m.Status == MatingStatus.Scheduled || m.Status == MatingStatus.Completed))
                .Take(2) // Schedule 2 per week
                .ToListAsync(stoppingToken);

            if (eligibleSows.Any())
            {
                var boars = await dbContext.Animals
                    .Where(a => a.Type == AnimalType.Boar && a.Status == AnimalStatus.Active)
                    .ToListAsync(stoppingToken);

                if (boars.Any())
                {
                    foreach (var sow in eligibleSows)
                    {
                        // Check if there's no existing task for this sow
                        var existingTask = await dbContext.TaskAssignments
                            .AnyAsync(t => t.TaskCategoryId == GetSystemCategoryId("Mating") && 
                                           t.AnimalId == sow.Id && 
                                           t.Status != FarmTaskStatus.Completed, 
                                     stoppingToken);

                        if (!existingTask)
                        {
                            // Create task for farm workers
                            var boar = boars.First(); // Choose the first available boar
                            var task = new TaskAssignment
                            {
                                Title = $"Schedule mating for Sow #{sow.TagNumber}",
                                Description = $"Prepare Sow #{sow.TagNumber} for mating with Boar #{boar.TagNumber}.",
                                DueDate = today.AddDays(1), // Schedule for tomorrow
                                Status = FarmTaskStatus.Pending,
                                Priority = TaskPriority.High,
                                TaskCategoryId = GetSystemCategoryId("Mating"),
                                AnimalId = sow.Id,
                                EmployeeId = 1 // Assign to first employee (could be more sophisticated)
                            };

                            dbContext.TaskAssignments.Add(task);
                            await dbContext.SaveChangesAsync(stoppingToken);

                            _logger.LogInformation("Created mating task for Sow #{SowTag} with Boar #{BoarTag}", sow.TagNumber, boar.TagNumber);
                        }
                    }
                }
            }
        }

        private async Task CreatePregnancyCheckTasks(ApplicationDbContext dbContext, DateTime today, DateTime tomorrow, CancellationToken stoppingToken)
        {
            // First pregnancy check (18-21 days)
            var matingsForFirstCheck = await dbContext.Matings
                .Include(m => m.Mother)
                .Where(m => m.Status == MatingStatus.Completed && 
                           !m.PregnancyCheck1Result.HasValue && 
                           m.ExpectedPregnancyCheck1.Date <= tomorrow.Date)
                .ToListAsync(stoppingToken);

            foreach (var mating in matingsForFirstCheck)
            {
                // Create task for first pregnancy check
                var existingTask = await dbContext.TaskAssignments
                    .AnyAsync(t => t.TaskCategoryId == GetSystemCategoryId("PregnancyCheck") && 
                                   t.Title.Contains("first pregnancy check") && 
                                   t.AnimalId == mating.MotherAnimalId && 
                                   t.Status != FarmTaskStatus.Completed, 
                             stoppingToken);

                if (!existingTask)
                {
                    var task = new TaskAssignment
                    {
                        Title = $"Perform first pregnancy check for Sow #{mating.Mother.TagNumber}",
                        Description = $"Check if Sow #{mating.Mother.TagNumber} is pregnant (18-21 days after mating on {mating.MatingDate.ToShortDateString()}).",
                        DueDate = mating.ExpectedPregnancyCheck1,
                        Status = FarmTaskStatus.Pending,
                        Priority = TaskPriority.High,
                        TaskCategoryId = GetSystemCategoryId("PregnancyCheck"),
                        AnimalId = mating.MotherAnimalId,
                        EmployeeId = 1 // Assign to first employee
                    };

                    dbContext.TaskAssignments.Add(task);
                    await dbContext.SaveChangesAsync(stoppingToken);

                    _logger.LogInformation("Created first pregnancy check task for Sow #{SowTag}", mating.Mother.TagNumber);
                }
            }

            // Second pregnancy check (42 days)
            var matingsForSecondCheck = await dbContext.Matings
                .Include(m => m.Mother)
                .Where(m => m.Status == MatingStatus.Completed && 
                           m.PregnancyCheck1Result == true && 
                           !m.PregnancyCheck2Result.HasValue && 
                           m.ExpectedPregnancyCheck2.Date <= tomorrow.Date)
                .ToListAsync(stoppingToken);

            foreach (var mating in matingsForSecondCheck)
            {
                // Create task for second pregnancy check
                var existingTask = await dbContext.TaskAssignments
                    .AnyAsync(t => t.TaskCategoryId == GetSystemCategoryId("PregnancyCheck") && 
                                   t.Title.Contains("second pregnancy check") && 
                                   t.AnimalId == mating.MotherAnimalId && 
                                   t.Status != FarmTaskStatus.Completed, 
                             stoppingToken);

                if (!existingTask)
                {
                    var task = new TaskAssignment
                    {
                        Title = $"Perform second pregnancy check for Sow #{mating.Mother.TagNumber}",
                        Description = $"Confirm pregnancy for Sow #{mating.Mother.TagNumber} (42 days after mating on {mating.MatingDate.ToShortDateString()}).",
                        DueDate = mating.ExpectedPregnancyCheck2,
                        Status = FarmTaskStatus.Pending,
                        Priority = TaskPriority.High,
                        TaskCategoryId = GetSystemCategoryId("PregnancyCheck"),
                        AnimalId = mating.MotherAnimalId,
                        EmployeeId = 1 // Assign to first employee
                    };

                    dbContext.TaskAssignments.Add(task);
                    await dbContext.SaveChangesAsync(stoppingToken);

                    _logger.LogInformation("Created second pregnancy check task for Sow #{SowTag}", mating.Mother.TagNumber);
                }
            }
        }

        private async Task CreateVaccinationTasks(ApplicationDbContext dbContext, DateTime today, DateTime tomorrow, CancellationToken stoppingToken)
        {
            // First vaccination (day 100)
            var matingsForFirstVaccination = await dbContext.Matings
                .Include(m => m.Mother)
                .Where(m => m.Status == MatingStatus.PregnancyConfirmed && 
                           !m.Vaccination1Completed && 
                           m.ExpectedVaccinationDate1.Date <= tomorrow.Date)
                .ToListAsync(stoppingToken);

            foreach (var mating in matingsForFirstVaccination)
            {
                // Create task for first vaccination
                var existingTask = await dbContext.TaskAssignments
                    .AnyAsync(t => t.TaskCategoryId == GetSystemCategoryId("Vaccination") && 
                                   t.Title.Contains("first vaccination") && 
                                   t.AnimalId == mating.MotherAnimalId && 
                                   t.Status != FarmTaskStatus.Completed, 
                             stoppingToken);

                if (!existingTask)
                {
                    var task = new TaskAssignment
                    {
                        Title = $"Administer first vaccination to pregnant Sow #{mating.Mother.TagNumber}",
                        Description = $"Vaccinate Sow #{mating.Mother.TagNumber} at day 100 of pregnancy.",
                        DueDate = mating.ExpectedVaccinationDate1,
                        Status = FarmTaskStatus.Pending,
                        Priority = TaskPriority.High,
                        TaskCategoryId = GetSystemCategoryId("Vaccination"),
                        AnimalId = mating.MotherAnimalId,
                        EmployeeId = 1 // Assign to first employee
                    };

                    dbContext.TaskAssignments.Add(task);
                    await dbContext.SaveChangesAsync(stoppingToken);

                    _logger.LogInformation("Created first vaccination task for Sow #{SowTag}", mating.Mother.TagNumber);
                }
            }

            // Second vaccination (day 107)
            var matingsForSecondVaccination = await dbContext.Matings
                .Include(m => m.Mother)
                .Where(m => m.Status == MatingStatus.PregnancyConfirmed && 
                           m.Vaccination1Completed && 
                           !m.Vaccination2Completed && 
                           m.ExpectedVaccinationDate2.Date <= tomorrow.Date)
                .ToListAsync(stoppingToken);

            foreach (var mating in matingsForSecondVaccination)
            {
                // Create task for second vaccination
                var existingTask = await dbContext.TaskAssignments
                    .AnyAsync(t => t.TaskCategoryId == GetSystemCategoryId("Vaccination") && 
                                   t.Title.Contains("second vaccination") && 
                                   t.AnimalId == mating.MotherAnimalId && 
                                   t.Status != FarmTaskStatus.Completed, 
                             stoppingToken);

                if (!existingTask)
                {
                    var task = new TaskAssignment
                    {
                        Title = $"Administer second vaccination to pregnant Sow #{mating.Mother.TagNumber}",
                        Description = $"Vaccinate Sow #{mating.Mother.TagNumber} at day 107 of pregnancy.",
                        DueDate = mating.ExpectedVaccinationDate2,
                        Status = FarmTaskStatus.Pending,
                        Priority = TaskPriority.High,
                        TaskCategoryId = GetSystemCategoryId("Vaccination"),
                        AnimalId = mating.MotherAnimalId,
                        EmployeeId = 1 // Assign to first employee
                    };

                    dbContext.TaskAssignments.Add(task);
                    await dbContext.SaveChangesAsync(stoppingToken);

                    _logger.LogInformation("Created second vaccination task for Sow #{SowTag}", mating.Mother.TagNumber);
                }
            }
        }

        private async Task CreatePigletProcessingTasks(ApplicationDbContext dbContext, DateTime today, CancellationToken stoppingToken)
        {
            // Get matings that have farrowed within the last 3 days (for piglet processing)
            var recentFarrowings = await dbContext.Matings
                .Include(m => m.Mother)
                .Where(m => m.Status == MatingStatus.Farrowed && 
                           m.ActualFarrowingDate.HasValue && 
                           m.ActualFarrowingDate.Value.AddDays(3) >= today &&
                           m.NumberOfPigletsBornAlive > 0)
                .ToListAsync(stoppingToken);

            foreach (var farrowing in recentFarrowings)
            {
                // Get the piglets from this farrowing
                var piglets = await dbContext.Animals
                    .Where(a => a.Type == AnimalType.Piglet && 
                              a.MotherAnimalId == farrowing.MotherAnimalId && 
                              a.BirthDate == farrowing.ActualFarrowingDate.Value)
                    .ToListAsync(stoppingToken);

                if (piglets.Any())
                {
                    // Check if there are processing records for the piglets
                    foreach (var piglet in piglets)
                    {
                        var processing = await dbContext.PigletProcessings
                            .FirstOrDefaultAsync(p => p.AnimalId == piglet.Id, stoppingToken);

                        if (processing == null)
                        {
                            // Create new processing record
                            processing = new PigletProcessing
                            {
                                AnimalId = piglet.Id
                            };
                            dbContext.PigletProcessings.Add(processing);
                            await dbContext.SaveChangesAsync(stoppingToken);
                        }

                        // Initial piglet processing (days 1-3)
                        if (!processing.TailDockingCompleted || !processing.IronInjectionCompleted || !processing.EarNotchingCompleted)
                        {
                            // Create task for initial processing
                            var existingTask = await dbContext.TaskAssignments
                                .AnyAsync(t => t.TaskCategoryId == GetSystemCategoryId("PigletProcessing") && 
                                             t.Title.Contains("initial processing") && 
                                             t.AnimalId == piglet.Id && 
                                             t.Status != FarmTaskStatus.Completed, 
                                         stoppingToken);

                            if (!existingTask)
                            {
                                var task = new TaskAssignment
                                {
                                    Title = $"Perform initial processing for Piglet #{piglet.TagNumber}",
                                    Description = $"Complete tail docking, iron injection, and ear notching for piglet born on {piglet.BirthDate.ToShortDateString()}.",
                                    DueDate = piglet.BirthDate.AddDays(3),
                                    Status = FarmTaskStatus.Pending,
                                    Priority = TaskPriority.High,
                                    TaskCategoryId = GetSystemCategoryId("PigletProcessing"),
                                    AnimalId = piglet.Id,
                                    EmployeeId = 1 // Assign to first employee
                                };

                                dbContext.TaskAssignments.Add(task);
                                await dbContext.SaveChangesAsync(stoppingToken);

                                _logger.LogInformation("Created initial processing task for Piglet #{PigletTag}", piglet.TagNumber);
                            }
                        }
                    }
                }
            }

            // Tattooing and vaccination (day 7)
            var pigletsDaySevenProcessing = await dbContext.Animals
                .Where(a => a.Type == AnimalType.Piglet && 
                          a.BirthDate.AddDays(7).Date == today.Date)
                .ToListAsync(stoppingToken);

            foreach (var piglet in pigletsDaySevenProcessing)
            {
                var processing = await dbContext.PigletProcessings
                    .FirstOrDefaultAsync(p => p.AnimalId == piglet.Id, stoppingToken);

                if (processing != null && (!processing.TattooingCompleted || !processing.InitialVaccinationCompleted))
                {
                    // Create task for day 7 processing
                    var existingTask = await dbContext.TaskAssignments
                        .AnyAsync(t => t.TaskCategoryId == GetSystemCategoryId("PigletProcessing") && 
                                     t.Title.Contains("day 7 processing") && 
                                     t.AnimalId == piglet.Id && 
                                     t.Status != FarmTaskStatus.Completed, 
                                 stoppingToken);

                    if (!existingTask)
                    {
                        var task = new TaskAssignment
                        {
                            Title = $"Perform day 7 processing for Piglet #{piglet.TagNumber}",
                            Description = $"Complete tattooing and initial vaccination for piglet.",
                            DueDate = piglet.BirthDate.AddDays(7),
                            Status = FarmTaskStatus.Pending,
                            Priority = TaskPriority.High,
                            TaskCategoryId = GetSystemCategoryId("PigletProcessing"),
                            AnimalId = piglet.Id,
                            EmployeeId = 1 // Assign to first employee
                        };

                        dbContext.TaskAssignments.Add(task);
                        await dbContext.SaveChangesAsync(stoppingToken);

                        _logger.LogInformation("Created day 7 processing task for Piglet #{PigletTag}", piglet.TagNumber);
                    }
                }
            }

            // Introduce to creep feed (day 10)
            var pigletsDayTenProcessing = await dbContext.Animals
                .Where(a => a.Type == AnimalType.Piglet && 
                          a.BirthDate.AddDays(10).Date == today.Date)
                .ToListAsync(stoppingToken);

            foreach (var piglet in pigletsDayTenProcessing)
            {
                var processing = await dbContext.PigletProcessings
                    .FirstOrDefaultAsync(p => p.AnimalId == piglet.Id, stoppingToken);

                if (processing != null && !processing.CreepFeedIntroductionCompleted)
                {
                    // Create task for creep feed introduction
                    var existingTask = await dbContext.TaskAssignments
                        .AnyAsync(t => t.TaskCategoryId == GetSystemCategoryId("Feeding") && 
                                     t.Title.Contains("creep feed") && 
                                     t.AnimalId == piglet.Id && 
                                     t.Status != FarmTaskStatus.Completed, 
                                 stoppingToken);

                    if (!existingTask)
                    {
                        var task = new TaskAssignment
                        {
                            Title = $"Introduce creep feed to Piglet #{piglet.TagNumber}",
                            Description = $"Begin introducing creep feed to piglet on day 10.",
                            DueDate = piglet.BirthDate.AddDays(10),
                            Status = FarmTaskStatus.Pending,
                            Priority = TaskPriority.Medium,
                            TaskCategoryId = GetSystemCategoryId("Feeding"),
                            AnimalId = piglet.Id,
                            EmployeeId = 2 // Assign to second employee
                        };

                        dbContext.TaskAssignments.Add(task);
                        await dbContext.SaveChangesAsync(stoppingToken);

                        _logger.LogInformation("Created creep feed introduction task for Piglet #{PigletTag}", piglet.TagNumber);
                    }
                }
            }

            // Weighing (day 21)
            var pigletsDayTwentyOneProcessing = await dbContext.Animals
                .Where(a => a.Type == AnimalType.Piglet && 
                          a.BirthDate.AddDays(21).Date == today.Date)
                .ToListAsync(stoppingToken);

            foreach (var piglet in pigletsDayTwentyOneProcessing)
            {
                var processing = await dbContext.PigletProcessings
                    .FirstOrDefaultAsync(p => p.AnimalId == piglet.Id, stoppingToken);

                if (processing != null && !processing.Weighing21DaysCompleted)
                {
                    // Create task for day 21 weighing
                    var existingTask = await dbContext.TaskAssignments
                        .AnyAsync(t => t.TaskCategoryId == GetSystemCategoryId("Weighing") && 
                                     t.Title.Contains("day 21 weighing") && 
                                     t.AnimalId == piglet.Id && 
                                     t.Status != FarmTaskStatus.Completed, 
                                 stoppingToken);

                    if (!existingTask)
                    {
                        var task = new TaskAssignment
                        {
                            Title = $"Weigh Piglet #{piglet.TagNumber} (day 21)",
                            Description = $"Weigh piglet at 21 days of age and record weight.",
                            DueDate = piglet.BirthDate.AddDays(21),
                            Status = FarmTaskStatus.Pending,
                            Priority = TaskPriority.Medium,
                            TaskCategoryId = GetSystemCategoryId("Weighing"),
                            AnimalId = piglet.Id,
                            EmployeeId = 2 // Assign to second employee
                        };

                        dbContext.TaskAssignments.Add(task);
                        await dbContext.SaveChangesAsync(stoppingToken);

                        _logger.LogInformation("Created day 21 weighing task for Piglet #{PigletTag}", piglet.TagNumber);
                    }
                }
            }

            // Weighing and weaning prep (day 28)
            var pigletsDayTwentyEightProcessing = await dbContext.Animals
                .Where(a => a.Type == AnimalType.Piglet && 
                          a.BirthDate.AddDays(28).Date == today.Date)
                .ToListAsync(stoppingToken);

            foreach (var piglet in pigletsDayTwentyEightProcessing)
            {
                var processing = await dbContext.PigletProcessings
                    .FirstOrDefaultAsync(p => p.AnimalId == piglet.Id, stoppingToken);

                if (processing != null && (!processing.Weighing28DaysCompleted || !processing.WeaningCompleted))
                {
                    // Create task for day 28 weighing and weaning
                    var existingTask = await dbContext.TaskAssignments
                        .AnyAsync(t => t.TaskCategoryId == GetSystemCategoryId("Weaning") && 
                                     t.Title.Contains("day 28 weighing and weaning") && 
                                     t.AnimalId == piglet.Id && 
                                     t.Status != FarmTaskStatus.Completed, 
                                 stoppingToken);

                    if (!existingTask)
                    {
                        var task = new TaskAssignment
                        {
                            Title = $"Day 28 weighing and weaning for Piglet #{piglet.TagNumber}",
                            Description = $"Weigh piglet at 28 days of age and complete weaning process.",
                            DueDate = piglet.BirthDate.AddDays(28),
                            Status = FarmTaskStatus.Pending,
                            Priority = TaskPriority.High,
                            TaskCategoryId = GetSystemCategoryId("Weaning"),
                            AnimalId = piglet.Id,
                            EmployeeId = 1 // Assign to first employee
                        };

                        dbContext.TaskAssignments.Add(task);
                        await dbContext.SaveChangesAsync(stoppingToken);

                        _logger.LogInformation("Created day 28 weighing and weaning task for Piglet #{PigletTag}", piglet.TagNumber);
                    }
                }
            }
        }

        private async Task CreateWeaningTasks(ApplicationDbContext dbContext, DateTime today, CancellationToken stoppingToken)
        {
            // Feed management for weaners (creep pellets for 2 weeks after weaning)
            var recentlyWeanedPiglets = await dbContext.PigletProcessings
                .Include(p => p.Animal)
                .Where(p => p.WeaningCompleted && 
                          p.WeaningDate.HasValue && 
                          p.WeaningDate.Value.AddDays(14) >= today && 
                          p.WeaningDate.Value <= today)
                .ToListAsync(stoppingToken);

            foreach (var pigletProcessing in recentlyWeanedPiglets)
            {
                // Create feeding task if the piglet is still within 2 weeks of weaning
                if (pigletProcessing.Animal != null)
                {
                    // Update the animal type to Weaner if it's still a Piglet
                    if (pigletProcessing.Animal.Type == AnimalType.Piglet)
                    {
                        pigletProcessing.Animal.Type = AnimalType.Weaner;
                        await dbContext.SaveChangesAsync(stoppingToken);
                    }

                    // Check if there's already a feeding task for today
                    var existingFeedingTask = await dbContext.TaskAssignments
                        .AnyAsync(t => t.TaskCategoryId == GetSystemCategoryId("Feeding") && 
                                     t.Title.Contains("creep pellets for weaner") && 
                                     t.AnimalId == pigletProcessing.AnimalId && 
                                     t.DueDate.Date == today.Date, 
                                 stoppingToken);

                    if (!existingFeedingTask)
                    {
                        var task = new TaskAssignment
                        {
                            Title = $"Provide creep pellets for weaner #{pigletProcessing.Animal.TagNumber}",
                            Description = $"Continue providing creep pellets to weaner during first 2 weeks after weaning.",
                            DueDate = today,
                            Status = FarmTaskStatus.Pending,
                            Priority = TaskPriority.Medium,
                            TaskCategoryId = GetSystemCategoryId("Feeding"),
                            AnimalId = pigletProcessing.AnimalId,
                            EmployeeId = 2 // Assign to second employee
                        };

                        dbContext.TaskAssignments.Add(task);
                        await dbContext.SaveChangesAsync(stoppingToken);

                        _logger.LogInformation("Created creep pellet feeding task for Weaner #{WeanerTag}", pigletProcessing.Animal.TagNumber);
                    }
                }
            }

            // Transition to weaner feed after 2 weeks
            var weanersToTransition = await dbContext.PigletProcessings
                .Include(p => p.Animal)
                .Where(p => p.WeaningCompleted && 
                          p.WeaningDate.HasValue && 
                          p.WeaningDate.Value.AddDays(14).Date == today.Date)
                .ToListAsync(stoppingToken);

            foreach (var pigletProcessing in weanersToTransition)
            {
                if (pigletProcessing.Animal != null)
                {
                    var task = new TaskAssignment
                    {
                        Title = $"Transition weaner #{pigletProcessing.Animal.TagNumber} to weaner feed",
                        Description = $"Begin providing weaner feed instead of creep pellets after 2 weeks post-weaning. Continue for 4 weeks.",
                        DueDate = today,
                        Status = FarmTaskStatus.Pending,
                        Priority = TaskPriority.Medium,
                        TaskCategoryId = GetSystemCategoryId("Feeding"),
                        AnimalId = pigletProcessing.AnimalId,
                        EmployeeId = 2 // Assign to second employee
                    };

                    dbContext.TaskAssignments.Add(task);
                    await dbContext.SaveChangesAsync(stoppingToken);

                    _logger.LogInformation("Created feed transition task for Weaner #{WeanerTag}", pigletProcessing.Animal.TagNumber);
                }
            }

            // Transition to grower feed after 6 weeks post-weaning (2 weeks creep + 4 weeks weaner)
            var weanersToGrower = await dbContext.PigletProcessings
                .Include(p => p.Animal)
                .Where(p => p.WeaningCompleted && 
                          p.WeaningDate.HasValue && 
                          p.WeaningDate.Value.AddDays(42).Date == today.Date)
                .ToListAsync(stoppingToken);

            foreach (var pigletProcessing in weanersToGrower)
            {
                if (pigletProcessing.Animal != null)
                {
                    // Update animal type to Grower
                    pigletProcessing.Animal.Type = AnimalType.Grower;
                    await dbContext.SaveChangesAsync(stoppingToken);

                    var task = new TaskAssignment
                    {
                        Title = $"Transition pig #{pigletProcessing.Animal.TagNumber} to grower feed",
                        Description = $"Begin providing grower feed. This pig is now transitioning from weaner to grower phase.",
                        DueDate = today,
                        Status = FarmTaskStatus.Pending,
                        Priority = TaskPriority.Medium,
                        TaskCategoryId = GetSystemCategoryId("Feeding"),
                        AnimalId = pigletProcessing.AnimalId,
                        EmployeeId = 2 // Assign to second employee
                    };

                    dbContext.TaskAssignments.Add(task);
                    await dbContext.SaveChangesAsync(stoppingToken);

                    _logger.LogInformation("Created grower transition task for Pig #{PigTag}", pigletProcessing.Animal.TagNumber);
                }
            }
        }

        private async Task CreateAbattoirScheduleTasks(ApplicationDbContext dbContext, DateTime today, CancellationToken stoppingToken)
        {
            // Check if we need to schedule an abattoir shipment (weekly schedule of 10 pigs)
            // First, check if there's already a shipment scheduled for this week
            var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
            var endOfWeek = startOfWeek.AddDays(6);

            var existingShipment = await dbContext.AbattoirShipments
                .AnyAsync(s => s.ShipmentDate >= startOfWeek && s.ShipmentDate <= endOfWeek, stoppingToken);

            // If no shipment is scheduled for this week and it's Monday (good day to plan), create one
            if (!existingShipment && today.DayOfWeek == DayOfWeek.Monday)
            {
                // Find 10 growers/finishers that are ready for abattoir
                var readyPigs = await dbContext.Animals
                    .Where(a => (a.Type == AnimalType.Grower || a.Type == AnimalType.Finisher) && 
                              a.Status == AnimalStatus.Active && 
                              a.BirthDate.AddDays(150) <= today) // Typically around 5 months old
                    .OrderBy(a => a.BirthDate)
                    .Take(10)
                    .ToListAsync(stoppingToken);

                if (readyPigs.Count == 10) // If we have enough pigs
                {
                    // Create a task to prepare the shipment
                    var shipmentPreparationTask = new TaskAssignment
                    {
                        Title = "Prepare weekly abattoir shipment",
                        Description = $"Prepare 10 pigs for shipment to abattoir on {today.AddDays(3).ToShortDateString()} (Thursday).",
                        DueDate = today.AddDays(2), // Due Wednesday
                        Status = FarmTaskStatus.Pending,
                        Priority = TaskPriority.High,
                        TaskCategoryId = GetSystemCategoryId("AbattoirShipment"),
                        EmployeeId = 1 // Assign to first employee
                    };

                    dbContext.TaskAssignments.Add(shipmentPreparationTask);
                    await dbContext.SaveChangesAsync(stoppingToken);

                    _logger.LogInformation("Created abattoir shipment preparation task for 10 pigs");

                    // Create a new shipment record
                    var shipment = new AbattoirShipment
                    {
                        ShipmentDate = today.AddDays(3), // Thursday
                        AbattoirName = "Local Township Abattoir",
                        NumberOfPigs = 10,
                        Status = ShipmentStatus.Scheduled,
                        EstimatedValue = 15000.00M, // Estimated value in ZAR
                        TransportationCost = 1000.00M // Estimated transport cost in ZAR
                    };

                    // Add pigs to shipment
                    foreach (var pig in readyPigs)
                    {
                        shipment.Animals.Add(pig);
                    }

                    dbContext.AbattoirShipments.Add(shipment);
                    await dbContext.SaveChangesAsync(stoppingToken);

                    _logger.LogInformation("Created new abattoir shipment scheduled for {ShipmentDate}", shipment.ShipmentDate);
                }
            }
        }

        private int GetSystemCategoryId(string categoryName)
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var category = dbContext.TaskCategories
                .FirstOrDefault(c => c.Name.Equals(categoryName, StringComparison.OrdinalIgnoreCase) && c.IsSystem);
            
            if (category == null)
            {
                // Create the system category if it doesn't exist
                category = new TaskCategory
                {
                    Name = categoryName,
                    Description = $"System category for {categoryName} tasks",
                    IsSystem = true
                };
                dbContext.TaskCategories.Add(category);
                dbContext.SaveChanges();
            }
            
            return category.Id;
        }
    }
} 