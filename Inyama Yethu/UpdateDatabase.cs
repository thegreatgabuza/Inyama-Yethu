using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Inyama_Yethu.Data;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using Inyama_Yethu.Models;
using Microsoft.Extensions.Logging;

namespace Inyama_Yethu
{
    public static class UpdateDatabase
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("Starting database update...");
            
            var serviceProvider = new ServiceCollection()
                .AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=InyamaYethuDB;Trusted_Connection=True;MultipleActiveResultSets=true"))
                .AddLogging()
                // Add Identity services
                .AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders()
                .Services
                .BuildServiceProvider();

            using (var scope = serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                Console.WriteLine("Applying migrations...");
                await dbContext.Database.MigrateAsync();
                
                // Initialize roles and admin user
                Console.WriteLine("Creating roles and admin user...");
                
                try 
                {
                    // Create Administrator role if it doesn't exist
                    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                    if (!await roleManager.RoleExistsAsync("Administrator"))
                    {
                        await roleManager.CreateAsync(new IdentityRole("Administrator"));
                        Console.WriteLine("Created Administrator role");
                    }
                    
                    // Create admin user if it doesn't exist
                    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
                    var adminEmail = "admin@inyamayethu.co.za";
                    var adminUser = await userManager.FindByEmailAsync(adminEmail);

                    if (adminUser == null)
                    {
                        adminUser = new IdentityUser
                        {
                            UserName = adminEmail,
                            Email = adminEmail,
                            EmailConfirmed = true // Auto-confirm email for admin
                        };

                        var password = "Admin@123456";
                        var result = await userManager.CreateAsync(adminUser, password);

                        if (result.Succeeded)
                        {
                            Console.WriteLine($"Created admin user with email {adminEmail}");
                            
                            // Assign admin role to user
                            await userManager.AddToRoleAsync(adminUser, "Administrator");
                            Console.WriteLine("Added admin user to Administrator role");
                        }
                        else
                        {
                            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                            Console.WriteLine($"Error creating admin user: {errors}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error creating roles and users: {ex.Message}");
                }
                
                Console.WriteLine("Database update complete!");
            }
        }

        public static async Task SeedJanuary2024DataAsync(IServiceProvider serviceProvider, ILogger logger)
        {
            try
            {
                var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
                var random = new Random();

                // Create breeding boars
                var boars = new List<Animal>
                {
                    new Animal
                    {
                        TagNumber = "B2024-01",
                        Type = AnimalType.Boar,
                        Gender = Gender.Male,
                        BirthDate = DateTime.Parse("2022-06-15"),
                        Status = AnimalStatus.Active,
                        Weight = 280.5,
                        Notes = "Primary breeding boar - Large White"
                    },
                    new Animal
                    {
                        TagNumber = "B2024-02",
                        Type = AnimalType.Boar,
                        Gender = Gender.Male,
                        BirthDate = DateTime.Parse("2022-08-20"),
                        Status = AnimalStatus.Active,
                        Weight = 265.0,
                        Notes = "Secondary breeding boar - Landrace"
                    }
                };

                await context.Animals.AddRangeAsync(boars);
                await context.SaveChangesAsync();

                // Create mature sows
                var sows = new List<Animal>();
                for (int i = 1; i <= 10; i++)
                {
                    var sow = new Animal
                    {
                        TagNumber = $"S2024-{i:D2}",
                        Type = AnimalType.Sow,
                        Gender = Gender.Female,
                        BirthDate = DateTime.Parse("2022-03-15").AddDays(random.Next(-30, 30)),
                        Status = AnimalStatus.Active,
                        Weight = 200 + random.Next(20, 40),
                        Notes = $"Mature breeding sow - Batch {(i-1)/5 + 1}"
                    };
                    sows.Add(sow);
                }

                await context.Animals.AddRangeAsync(sows);
                await context.SaveChangesAsync();

                // Create farrowing records for January 2024
                var farrowingRecords = new List<Birth>();
                var startDate = DateTime.Parse("2024-01-05");

                // Create farrowing records for first 5 sows
                for (int i = 0; i < 5; i++)
                {
                    var birthDate = startDate.AddDays(i * 2); // Spread births across January
                    var pigletCount = random.Next(8, 15); // Random litter size between 8-14 piglets
                    
                    var birth = new Birth
                    {
                        MotherAnimalId = sows[i].Id,
                        FatherAnimalId = boars[random.Next(2)].Id,
                        BirthDate = birthDate,
                        LitterSize = pigletCount,
                        LiveBorn = pigletCount - random.Next(0, 2), // Some may be stillborn
                        Status = BirthStatus.Normal,
                        Notes = $"January 2024 farrowing - Batch 1",
                        AverageBirthWeight = 1.4 + random.NextDouble() * 0.4 // Random average between 1.4-1.8 kg
                    };

                    // Create piglets
                    for (int j = 1; j <= birth.LiveBorn; j++)
                    {
                        var piglet = new Animal
                        {
                            TagNumber = $"P2024-{i+1}-{j:D2}",
                            Type = AnimalType.Piglet,
                            Gender = random.Next(2) == 0 ? Gender.Male : Gender.Female,
                            BirthDate = birthDate,
                            Status = AnimalStatus.Active,
                            Weight = 1.2 + random.NextDouble() * 0.6, // Random weight between 1.2-1.8 kg
                            MotherAnimalId = sows[i].Id,
                            FatherAnimalId = birth.FatherAnimalId,
                            Notes = $"Born to sow {sows[i].TagNumber}"
                        };
                        await context.Animals.AddAsync(piglet);
                    }

                    farrowingRecords.Add(birth);
                }

                await context.Births.AddRangeAsync(farrowingRecords);
                await context.SaveChangesAsync();

                logger.LogInformation("Successfully seeded January 2024 animals and farrowing records");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while seeding January 2024 data");
                throw;
            }
        }
    }
} 