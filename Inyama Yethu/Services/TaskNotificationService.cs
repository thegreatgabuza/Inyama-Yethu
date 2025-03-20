using System;
using System.Threading.Tasks;
using Inyama_Yethu.Models;
using Microsoft.Extensions.Configuration;

namespace Inyama_Yethu.Services
{
    public interface ITaskNotificationService
    {
        Task SendTaskAssignmentNotificationAsync(TaskAssignment task, string employeeEmail);
        Task SendTaskDueDateReminderAsync(TaskAssignment task, string employeeEmail);
        Task SendTaskStatusChangeNotificationAsync(TaskAssignment task, string employeeEmail, string previousStatus);
        Task SendTaskOverdueNotificationAsync(TaskAssignment task, string employeeEmail);
        Task SendTaskReassignmentNotificationAsync(TaskAssignment task, string newEmployeeEmail, string previousEmployeeEmail);
    }

    public class TaskNotificationService : ITaskNotificationService
    {
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;

        public TaskNotificationService(IEmailService emailService, IConfiguration configuration)
        {
            _emailService = emailService;
            _configuration = configuration;
        }

        public async Task SendTaskAssignmentNotificationAsync(TaskAssignment task, string employeeEmail)
        {
            var subject = $"New Task Assignment: {task.Title}";
            var message = $@"
                <h2>New Task Assignment</h2>
                <p>You have been assigned a new task:</p>
                <p><strong>Title:</strong> {task.Title}</p>
                <p><strong>Description:</strong> {task.Description}</p>
                <p><strong>Due Date:</strong> {task.DueDate:dd MMMM yyyy}</p>
                <p><strong>Priority:</strong> {task.Priority}</p>
                <p><strong>Category:</strong> {task.Category?.Name}</p>
                {(task.AnimalId.HasValue ? $"<p><strong>Related Animal ID:</strong> {task.AnimalId}</p>" : "")}
                <p><strong>Status:</strong> {task.Status}</p>
                <br>
                <p>Please log in to the system to view more details and start working on this task.</p>";

            await _emailService.SendEmailAsync(employeeEmail, subject, message);
        }

        public async Task SendTaskDueDateReminderAsync(TaskAssignment task, string employeeEmail)
        {
            var subject = $"Task Due Soon: {task.Title}";
            var message = $@"
                <h2>Task Due Date Reminder</h2>
                <p>This is a reminder that the following task is due soon:</p>
                <p><strong>Title:</strong> {task.Title}</p>
                <p><strong>Due Date:</strong> {task.DueDate:dd MMMM yyyy}</p>
                <p><strong>Current Status:</strong> {task.Status}</p>
                <br>
                <p>Please ensure to complete the task before the due date.</p>";

            await _emailService.SendEmailAsync(employeeEmail, subject, message);
        }

        public async Task SendTaskStatusChangeNotificationAsync(TaskAssignment task, string employeeEmail, string previousStatus)
        {
            var subject = $"Task Status Updated: {task.Title}";
            var message = $@"
                <h2>Task Status Update</h2>
                <p>The status of your task has been updated:</p>
                <p><strong>Title:</strong> {task.Title}</p>
                <p><strong>Previous Status:</strong> {previousStatus}</p>
                <p><strong>New Status:</strong> {task.Status}</p>
                <p><strong>Due Date:</strong> {task.DueDate:dd MMMM yyyy}</p>
                <br>
                <p>Please log in to the system to view more details.</p>";

            await _emailService.SendEmailAsync(employeeEmail, subject, message);
        }

        public async Task SendTaskOverdueNotificationAsync(TaskAssignment task, string employeeEmail)
        {
            var subject = $"OVERDUE Task Alert: {task.Title}";
            var message = $@"
                <h2>Task Overdue Alert</h2>
                <p>The following task is now overdue:</p>
                <p><strong>Title:</strong> {task.Title}</p>
                <p><strong>Due Date:</strong> {task.DueDate:dd MMMM yyyy}</p>
                <p><strong>Current Status:</strong> {task.Status}</p>
                <br>
                <p>Please update the task status or contact your supervisor if you need assistance.</p>";

            await _emailService.SendEmailAsync(employeeEmail, subject, message);
        }

        public async Task SendTaskReassignmentNotificationAsync(TaskAssignment task, string newEmployeeEmail, string previousEmployeeEmail)
        {
            // Notify new assignee
            var newAssigneeSubject = $"Task Reassigned to You: {task.Title}";
            var newAssigneeMessage = $@"
                <h2>Task Reassignment Notice</h2>
                <p>A task has been reassigned to you:</p>
                <p><strong>Title:</strong> {task.Title}</p>
                <p><strong>Description:</strong> {task.Description}</p>
                <p><strong>Due Date:</strong> {task.DueDate:dd MMMM yyyy}</p>
                <p><strong>Current Status:</strong> {task.Status}</p>
                <br>
                <p>Please log in to the system to view more details and begin working on this task.</p>";

            await _emailService.SendEmailAsync(newEmployeeEmail, newAssigneeSubject, newAssigneeMessage);

            // Notify previous assignee
            var previousAssigneeSubject = $"Task Reassignment Notice: {task.Title}";
            var previousAssigneeMessage = $@"
                <h2>Task Reassignment Notice</h2>
                <p>The following task has been reassigned:</p>
                <p><strong>Title:</strong> {task.Title}</p>
                <p><strong>Description:</strong> {task.Description}</p>
                <p><strong>Due Date:</strong> {task.DueDate:dd MMMM yyyy}</p>
                <br>
                <p>This task is no longer assigned to you.</p>";

            await _emailService.SendEmailAsync(previousEmployeeEmail, previousAssigneeSubject, previousAssigneeMessage);
        }
    }
} 