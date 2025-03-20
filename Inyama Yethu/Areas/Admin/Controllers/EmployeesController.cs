using Inyama_Yethu.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using EmployeeModel = Inyama_Yethu.Models.Employee;
using Inyama_Yethu.Models;
using Inyama_Yethu.Services;

namespace Inyama_Yethu.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/[controller]")]
    [Authorize(Roles = "Administrator")]
    public class EmployeesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;

        public EmployeesController(ApplicationDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        // GET: Admin/Employees
        [HttpGet]
        [Route("")]
        [Route("Index")]
        public async Task<IActionResult> Index()
        {
            var employees = await _context.Employees
                .OrderByDescending(e => e.IsActive)
                .ThenBy(e => e.FirstName)
                .ThenBy(e => e.LastName)
                .ToListAsync();

            return View("~/Areas/Admin/Views/Employees/Index.cshtml", employees);
        }

        // GET: Admin/Employees/Details/5
        [HttpGet]
        [Route("Details/{id}")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees
                .Include(e => e.Attendances.OrderByDescending(a => a.CheckInTime).Take(10))
                .Include(e => e.TaskAssignments.Where(t => t.Status != FarmTaskStatus.Completed).Take(10))
                .FirstOrDefaultAsync(m => m.Id == id);

            if (employee == null)
            {
                return NotFound();
            }

            return View("~/Areas/Admin/Views/Employees/Details.cshtml", employee);
        }

        // GET: Admin/Employees/Create
        [HttpGet]
        [Route("Create")]
        public IActionResult Create()
        {
            return View("~/Areas/Admin/Views/Employees/Create.cshtml");
        }

        // POST: Admin/Employees/Create
        [HttpPost]
        [Route("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FirstName,LastName,Email,PhoneNumber,DateOfBirth,HireDate,JobTitle,Position,Department,Status,IsActive")] EmployeeModel employee)
        {
            if (ModelState.IsValid)
            {
                _context.Add(employee);
                await _context.SaveChangesAsync();

                // Generate a temporary password
                var tempPassword = GenerateTemporaryPassword();

                // Send welcome email with login credentials
                var subject = "Welcome to Inyama Yethu - Your Login Credentials";
                var message = $@"
                    <h2>Welcome to Inyama Yethu, {employee.FirstName}!</h2>
                    <p>Your account has been created successfully. Below are your login credentials:</p>
                    <p><strong>Email:</strong> {employee.Email}</p>
                    <p><strong>Temporary Password:</strong> {tempPassword}</p>
                    <p>Please change your password after your first login for security purposes.</p>
                    <p>If you have any questions, please contact your administrator.</p>
                    <br>
                    <p>Best regards,</p>
                    <p>Inyama Yethu Team</p>";

                await _emailService.SendEmailAsync(employee.Email, subject, message);

                return RedirectToAction(nameof(Index));
            }
            return View(employee);
        }

        private string GenerateTemporaryPassword()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789!@#$%^&*";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 12)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        // GET: Admin/Employees/Edit/5
        [HttpGet]
        [Route("Edit/{id}")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees
                .Include(e => e.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (employee == null)
            {
                return NotFound();
            }
            return View("~/Areas/Admin/Views/Employees/Edit.cshtml", employee);
        }

        // POST: Admin/Employees/Edit/5
        [HttpPost]
        [Route("Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FirstName,LastName,Email,PhoneNumber,JobTitle,DateOfBirth,HireDate,IsActive,UserId,Address")] EmployeeModel employee)
        {
            if (id != employee.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingEmployee = await _context.Employees
                        .Include(e => e.User)
                        .FirstOrDefaultAsync(e => e.Id == id);

                    if (existingEmployee == null)
                    {
                        return NotFound();
                    }

                    // Update the user if it exists
                    if (existingEmployee.User != null)
                    {
                        existingEmployee.User.Email = employee.Email;
                        existingEmployee.User.UserName = employee.Email;
                        existingEmployee.User.PhoneNumber = employee.PhoneNumber;
                    }

                    // Update employee properties
                    existingEmployee.FirstName = employee.FirstName;
                    existingEmployee.LastName = employee.LastName;
                    existingEmployee.Email = employee.Email;
                    existingEmployee.PhoneNumber = employee.PhoneNumber;
                    existingEmployee.JobTitle = employee.JobTitle;
                    existingEmployee.DateOfBirth = employee.DateOfBirth;
                    existingEmployee.HireDate = employee.HireDate;
                    existingEmployee.IsActive = employee.IsActive;
                    existingEmployee.Address = employee.Address;

                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Employee updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmployeeExists(employee.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View("~/Areas/Admin/Views/Employees/Edit.cshtml", employee);
        }

        // GET: Admin/Employees/Delete/5
        [HttpGet]
        [Route("Delete/{id}")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees
                .FirstOrDefaultAsync(m => m.Id == id);
            if (employee == null)
            {
                return NotFound();
            }

            return View("~/Areas/Admin/Views/Employees/Delete.cshtml", employee);
        }

        // POST: Admin/Employees/Delete/5
        [HttpPost]
        [Route("Delete/{id}")]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var employee = await _context.Employees
                .Include(e => e.User)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (employee == null)
            {
                return NotFound();
            }
            
            // Instead of deleting, just mark as inactive
            employee.IsActive = false;
            
            // If there's an associated user account, disable it
            if (employee.User != null)
            {
                employee.User.LockoutEnd = DateTimeOffset.MaxValue;
            }
            
            await _context.SaveChangesAsync();

            // Send deactivation notification
            var subject = "Account Deactivated - Inyama Yethu";
            var message = $@"
                <h2>Dear {employee.FirstName},</h2>
                <p>Your account at Inyama Yethu has been deactivated.</p>
                <p>If you believe this is an error, please contact your administrator.</p>
                <br>
                <p>Best regards,</p>
                <p>Inyama Yethu Team</p>";

            await _emailService.SendEmailAsync(employee.Email, subject, message);
            
            TempData["SuccessMessage"] = "Employee has been deactivated successfully.";
            return RedirectToAction(nameof(Index));
        }
        
        // GET: Admin/Employees/Activate/5
        [HttpGet]
        [Route("Activate/{id}")]
        public async Task<IActionResult> Activate(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (employee == null)
            {
                return NotFound();
            }

            return View("~/Areas/Admin/Views/Employees/Delete.cshtml", employee);
        }
        
        // POST: Admin/Employees/Activate/5
        [HttpPost]
        [Route("Activate/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActivateConfirmed(int id)
        {
            var employee = await _context.Employees
                .Include(e => e.User)
                .FirstOrDefaultAsync(e => e.Id == id);
            
            if (employee == null)
            {
                return NotFound();
            }
            
            employee.IsActive = true;
            
            // If there's an associated user account, enable it
            if (employee.User != null)
            {
                employee.User.LockoutEnd = null;
            }
            
            await _context.SaveChangesAsync();

            // Send reactivation notification
            var subject = "Account Reactivated - Inyama Yethu";
            var message = $@"
                <h2>Welcome back, {employee.FirstName}!</h2>
                <p>Your account at Inyama Yethu has been reactivated.</p>
                <p>You can now log in to the system using your previous credentials.</p>
                <p>If you have forgotten your password, please use the 'Forgot Password' option on the login page.</p>
                <br>
                <p>Best regards,</p>
                <p>Inyama Yethu Team</p>";

            await _emailService.SendEmailAsync(employee.Email, subject, message);
            
            TempData["SuccessMessage"] = "Employee has been reactivated successfully.";
            return RedirectToAction(nameof(Index));
        }

        private bool EmployeeExists(int id)
        {
            return _context.Employees.Any(e => e.Id == id);
        }
    }
} 