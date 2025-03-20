using Inyama_Yethu.Areas.Admin;
using Inyama_Yethu.Areas.Customer;
using Inyama_Yethu.Areas.Employee;
using Inyama_Yethu.Conventions;
using Inyama_Yethu.Data;
using Inyama_Yethu.Models;
using Inyama_Yethu.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString, sqlServerOptionsAction: sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null);
    }));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Configure Identity options
builder.Services.Configure<IdentityOptions>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 8;
    options.Password.RequiredUniqueChars = 1;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = true;
});

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

builder.Services.AddControllersWithViews(options => 
{
    // Register area conventions
    options.Conventions.Add(new AdminAreaRegistration());
    options.Conventions.Add(new EmployeeAreaRegistration());
    options.Conventions.Add(new CustomerAreaRegistration());
});

builder.Services.AddRazorPages();

// Register TimeZoneInfo for South Africa
builder.Services.AddSingleton(TimeZoneInfo.FindSystemTimeZoneById("South Africa Standard Time"));

// Add the TaskSchedulerService as a hosted service
builder.Services.AddHostedService<TaskSchedulerService>();

builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ITaskNotificationService, TaskNotificationService>();
builder.Services.AddHostedService<TaskReminderService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Map Identity endpoints
app.MapRazorPages();

// Add a specific route for the Admin area
app.MapControllerRoute(
    name: "admin",
    pattern: "Admin/{controller=Dashboard}/{action=Index}/{id?}",
    defaults: new { area = "Admin" });

// Map area routes
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

// Map default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Seed roles and admin user
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        context.Database.EnsureCreated();
        
        if (context.Database.GetPendingMigrations().Any())
        {
            logger.LogInformation("Applying pending migrations...");
            context.Database.Migrate();
            logger.LogInformation("Migrations applied successfully.");
        }

        // Create roles
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var roles = new[] { "Administrator", "Employee", "Customer" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
                logger.LogInformation("Created role {Role}", role);
            }
        }

        // Create admin user
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
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

        // Create default employee users for the two farm workers
        var defaultEmployees = new[]
        {
            new { Email = "", FirstName = "John", LastName = "Doe" },
            new { Email = "jane.smith@inyamayethu.co.za", FirstName = "Jane", LastName = "Smith" }
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

                    // Create the employee record
                    var employee = new Inyama_Yethu.Models.Employee
                    {
                        FirstName = emp.FirstName,
                        LastName = emp.LastName,
                        Email = emp.Email,
                        UserId = employeeUser.Id,
                        IsActive = true,
                        HireDate = DateTime.Now,
                        JobTitle = "Farm Worker",
                        Address = "123 Farm Road, Inyama Yethu",
                        DateOfBirth = new DateTime(1990, 1, 1),
                        PhoneNumber = "0123456789"
                    };
                    context.Employees.Add(employee);
                    await context.SaveChangesAsync();
                }
                else
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    logger.LogError("Error creating employee user {Name}: {Errors}", $"{emp.FirstName} {emp.LastName}", errors);
                }
            }
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while seeding roles and users");
    }
}

// Seed task categories
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    await TaskCategorySeeder.SeedDefaultCategoriesAsync(context);
}

app.Run();
