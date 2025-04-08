using System;
using System.ComponentModel.DataAnnotations;
using Inyama_Yethu.Models;

namespace Inyama_Yethu.ViewModels
{
    public class TaskAssignmentViewModel
    {
        [Required]
        [Display(Name = "Task Title")]
        public string Title { get; set; }

        [Required]
        [Display(Name = "Description")]
        public string Description { get; set; }

        [Required]
        [Display(Name = "Due Date")]
        [DataType(DataType.DateTime)]
        public DateTime DueDate { get; set; }

        [Required]
        [Display(Name = "Assigned Employee")]
        public int EmployeeId { get; set; }

        [Display(Name = "Priority")]
        public TaskPriority Priority { get; set; }

        [Display(Name = "Related Animal")]
        public int? AnimalId { get; set; }

        [Display(Name = "Category")]
        public int? CategoryId { get; set; }
    }

    public class TaskTemplateViewModel
    {
        [Required]
        [Display(Name = "Template Name")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Description")]
        public string Description { get; set; }

        [Display(Name = "Category")]
        public int? CategoryId { get; set; }

        [Display(Name = "Default Priority")]
        public TaskPriority DefaultPriority { get; set; }

        [Display(Name = "Estimated Duration (hours)")]
        [Range(0.5, 24)]
        public double EstimatedDuration { get; set; }

        [Display(Name = "Is Recurring")]
        public bool IsRecurring { get; set; }

        [Display(Name = "Recurrence Pattern")]
        public RecurrencePattern? RecurrencePattern { get; set; }
    }

    public class RoutineTaskViewModel
    {
        [Required]
        [Display(Name = "Task Title")]
        public string Title { get; set; }

        [Required]
        [Display(Name = "Description")]
        public string Description { get; set; }

        [Required]
        [Display(Name = "Start Date")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required]
        [Display(Name = "Frequency")]
        public TaskFrequency Frequency { get; set; }

        [Display(Name = "End Date")]
        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }

        [Display(Name = "Category")]
        public int? CategoryId { get; set; }

        [Display(Name = "Priority")]
        public TaskPriority Priority { get; set; }
    }

    public enum TaskFrequency
    {
        Daily,
        Weekly,
        BiWeekly,
        Monthly
    }

    public enum RecurrencePattern
    {
        Daily,
        Weekly,
        Monthly,
        Custom
    }
} 