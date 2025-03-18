using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Inyama_Yethu.Data;
using Microsoft.AspNetCore.Identity;
using System.Linq;

namespace Inyama_Yethu
{
    public class UpdateDatabase
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
    }
} 