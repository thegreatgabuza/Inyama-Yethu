using Inyama_Yethu.Data;
using Inyama_Yethu.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Inyama_Yethu.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/[controller]")]
    [Authorize(Roles = "Administrator")]
    public class EmployeesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EmployeesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Employees
        [HttpGet]
        [Route("")]
        [Route("Index")]
        public async Task<IActionResult> Index()
        {
            var employees = await _context.Employees
                .OrderByDescending(e => e.IsActive)
                .ThenBy(e => e.FullName)
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
        public async Task<IActionResult> Create([Bind("FirstName,LastName,Email,PhoneNumber,JobTitle,HireDate,IsActive")] Inyama_Yethu.Models.Employee employee)
        {
            if (ModelState.IsValid)
            {
                _context.Add(employee);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Employee created successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View("~/Areas/Admin/Views/Employees/Create.cshtml", employee);
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

            var employee = await _context.Employees.FindAsync(id);
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
        public async Task<IActionResult> Edit(int id, [Bind("Id,FirstName,LastName,Email,PhoneNumber,JobTitle,HireDate,IsActive")] Inyama_Yethu.Models.Employee employee)
        {
            if (id != employee.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(employee);
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
            var employee = await _context.Employees.FindAsync(id);
            
            // Instead of deleting, just mark as inactive
            employee.IsActive = false;
            
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Employee marked as inactive.";
            return RedirectToAction(nameof(Index));
        }
        
        // POST: Admin/Employees/Activate/5
        [HttpPost]
        [Route("Activate/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Activate(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            
            if (employee == null)
            {
                return NotFound();
            }
            
            employee.IsActive = true;
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Employee activated successfully.";
            return RedirectToAction(nameof(Index));
        }

        private bool EmployeeExists(int id)
        {
            return _context.Employees.Any(e => e.Id == id);
        }
    }
} 