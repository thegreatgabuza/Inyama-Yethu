using Inyama_Yethu.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Inyama_Yethu.Services
{
    public class TaskReminderService : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly TimeZoneInfo _southAfricaTimeZone;

        public TaskReminderService(IServiceProvider services, TimeZoneInfo timeZoneInfo)
        {
            _services = services;
            _southAfricaTimeZone = timeZoneInfo;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _services.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    var notificationService = scope.ServiceProvider.GetRequiredService<ITaskNotificationService>();

                    var now = TimeZoneInfo.ConvertTime(DateTime.Now, _southAfricaTimeZone);
                    var today = now.Date;
                    var tomorrow = today.AddDays(1);

                    // Get tasks due tomorrow that haven't been reminded yet
                    var tasksDueTomorrow = await dbContext.TaskAssignments
                        .Include(t => t.Employee)
                        .Where(t => t.DueDate.Date == tomorrow &&
                                  t.Status != Models.FarmTaskStatus.Completed &&
                                  t.Status != Models.FarmTaskStatus.Cancelled)
                        .ToListAsync(stoppingToken);

                    foreach (var task in tasksDueTomorrow)
                    {
                        if (task.Employee?.Email != null)
                        {
                            await notificationService.SendTaskDueDateReminderAsync(task, task.Employee.Email);
                        }
                    }

                    // Get overdue tasks that haven't been notified yet
                    var overdueTasks = await dbContext.TaskAssignments
                        .Include(t => t.Employee)
                        .Where(t => t.DueDate.Date < today &&
                                  t.Status != Models.FarmTaskStatus.Completed &&
                                  t.Status != Models.FarmTaskStatus.Cancelled)
                        .ToListAsync(stoppingToken);

                    foreach (var task in overdueTasks)
                    {
                        if (task.Employee?.Email != null)
                        {
                            await notificationService.SendTaskOverdueNotificationAsync(task, task.Employee.Email);
                        }
                    }
                }

                // Run checks once every hour
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }
    }
} 