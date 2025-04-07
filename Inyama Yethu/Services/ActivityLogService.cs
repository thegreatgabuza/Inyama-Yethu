using Inyama_Yethu.Data;
using Inyama_Yethu.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Inyama_Yethu.Services
{
    public interface IActivityLogService
    {
        Task LogActivityAsync(string entityName, int entityId, ActivityType activityType, string description, string oldValues = null, string newValues = null);
    }

    public class ActivityLogService : IActivityLogService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly TimeZoneInfo _southAfricaTimeZone;

        public ActivityLogService(
            ApplicationDbContext context,
            IHttpContextAccessor httpContextAccessor,
            UserManager<IdentityUser> userManager,
            TimeZoneInfo timeZoneInfo)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
            _southAfricaTimeZone = timeZoneInfo;
        }

        public async Task LogActivityAsync(string entityName, int entityId, ActivityType activityType, string description, string oldValues = null, string newValues = null)
        {
            var userId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userName = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Name);
            var userEmail = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Email);

            // If no username, use email instead
            if (string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(userEmail))
            {
                userName = userEmail;
            }

            // Try to find the employee ID if this is an employee user
            int? employeeId = null;
            if (!string.IsNullOrEmpty(userEmail))
            {
                var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Email == userEmail && e.IsActive);
                if (employee != null)
                {
                    employeeId = employee.Id;
                }
            }

            var activityLog = new ActivityLog
            {
                EntityName = entityName,
                EntityId = entityId,
                ActivityType = activityType,
                Description = description,
                UserId = userId,
                UserName = userName,
                EmployeeId = employeeId,
                Timestamp = TimeZoneInfo.ConvertTime(DateTime.Now, _southAfricaTimeZone),
                OldValues = oldValues,
                NewValues = newValues
            };

            _context.ActivityLogs.Add(activityLog);
            await _context.SaveChangesAsync();
        }
    }
} 