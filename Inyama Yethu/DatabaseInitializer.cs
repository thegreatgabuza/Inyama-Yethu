using Inyama_Yethu.Data;
using Inyama_Yethu.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Inyama_Yethu
{
    public static class DatabaseInitializer
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider, ILogger logger)
        {
            try
            {
                var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
                
                // Ensure database is created
                bool wasCreated = await context.Database.EnsureCreatedAsync();
                if (wasCreated)
                {
                    logger.LogInformation("Database was created");
                }
                else
                {
                    logger.LogInformation("Database already exists");
                }

                // Apply pending migrations
                if ((await context.Database.GetPendingMigrationsAsync()).Any())
                {
                    logger.LogInformation("Applying pending migrations...");
                    await context.Database.MigrateAsync();
                    logger.LogInformation("Migrations applied successfully.");
                }
                else
                {
                    logger.LogInformation("No pending migrations found");
                }

                // Initialize roles
                await InitializeRolesAsync(serviceProvider, logger);

                // Initialize users
                await InitializeAdminUserAsync(serviceProvider, logger);
                await InitializeEmployeeUsersAsync(serviceProvider, logger);

                // Seed initial pigs
                await SeedInitialPigsAsync(serviceProvider, logger);

                logger.LogInformation("Database initialization completed successfully");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred during database initialization");
                throw;
            }
        }

        private static async Task InitializeRolesAsync(IServiceProvider serviceProvider, ILogger logger)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var roles = new[] { "Administrator", "Employee", "Customer" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                    logger.LogInformation("Created role {Role}", role);
                }
                else
                {
                    logger.LogInformation("Role {Role} already exists", role);
                }
            }
        }

        private static async Task InitializeAdminUserAsync(IServiceProvider serviceProvider, ILogger logger)
        {
            // Create admin user
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
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

                // Use a secure password in production!
                var password = "Admin@123456";
                var result = await userManager.CreateAsync(adminUser, password);

                if (result.Succeeded)
                {
                    logger.LogInformation("Created admin user with email {Email}", adminEmail);
                    
                    // Assign admin role to user
                    await userManager.AddToRoleAsync(adminUser, "Administrator");
                    logger.LogInformation("Added admin user to Administrator role");
                }
                else
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    logger.LogError("Error creating admin user: {Errors}", errors);
                }
            }
            else
            {
                logger.LogInformation("Admin user {Email} already exists", adminEmail);
            }
        }

        private static async Task InitializeEmployeeUsersAsync(IServiceProvider serviceProvider, ILogger logger)
        {
            // Create default employee users for the two farm workers
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var defaultEmployees = new[]
            {
                new { Email = "john.doe@inyamayethu.co.za", Name = "John Doe" },
                new { Email = "jane.smith@inyamayethu.co.za", Name = "Jane Smith" }
            };

            foreach (var emp in defaultEmployees)
            {
                var employeeUser = await userManager.FindByEmailAsync(emp.Email);
                
                if (employeeUser == null)
                {
                    employeeUser = new IdentityUser
                    {
                        UserName = emp.Email,
                        Email = emp.Email,
                        EmailConfirmed = true
                    };

                    // Use a secure password in production!
                    var password = "Employee@123456";
                    var result = await userManager.CreateAsync(employeeUser, password);

                    if (result.Succeeded)
                    {
                        logger.LogInformation("Created employee user with email {Email}", emp.Email);
                        
                        // Assign employee role
                        await userManager.AddToRoleAsync(employeeUser, "Employee");
                        logger.LogInformation("Added user to Employee role");
                    }
                    else
                    {
                        var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                        logger.LogError("Error creating employee user {Name}: {Errors}", emp.Name, errors);
                    }
                }
                else
                {
                    logger.LogInformation("Employee user {Email} already exists", emp.Email);
                }
            }
        }

        private static async Task SeedInitialPigsAsync(IServiceProvider serviceProvider, ILogger logger)
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            
            // Check if pigs already exist in the database
            if (!await context.Animals.AnyAsync())
            {
                logger.LogInformation("Seeding initial pig data...");
                
                var random = new Random();
                
                // Create 2 boars
                var boar1 = new Animal
                {
                    TagNumber = "B001",
                    Type = AnimalType.Boar,
                    Gender = Gender.Male,
                    BirthDate = DateTime.Now.AddYears(-2),
                    Status = AnimalStatus.Active,
                    Weight = 250 + random.Next(0, 50),
                    Notes = "Mature breeding boar"
                };
                
                var boar2 = new Animal
                {
                    TagNumber = "B002",
                    Type = AnimalType.Boar,
                    Gender = Gender.Male,
                    BirthDate = DateTime.Now.AddYears(-1).AddMonths(-6),
                    Status = AnimalStatus.Active,
                    Weight = 230 + random.Next(0, 40),
                    Notes = "Mature breeding boar"
                };
                
                // Create 8 sows
                var sows = new List<Animal>();
                for (int i = 1; i <= 8; i++)
                {
                    var sow = new Animal
                    {
                        TagNumber = $"S00{i}",
                        Type = AnimalType.Sow,
                        Gender = Gender.Female,
                        BirthDate = DateTime.Now.AddYears(-1).AddDays(-random.Next(0, 200)),
                        Status = AnimalStatus.Active,
                        Weight = 180 + random.Next(0, 40),
                        Notes = "Mature breeding sow"
                    };
                    sows.Add(sow);
                }
                
                // Add all animals to the context
                context.Animals.Add(boar1);
                context.Animals.Add(boar2);
                foreach (var sow in sows)
                {
                    context.Animals.Add(sow);
                }
                
                // Save changes to the database
                await context.SaveChangesAsync();
                logger.LogInformation("Successfully seeded 10 pigs (2 boars and 8 sows)");
            }
            else
            {
                logger.LogInformation("Animals already exist in the database");
            }
        }
    }
} 