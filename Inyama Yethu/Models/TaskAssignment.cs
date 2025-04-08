using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inyama_Yethu.Models
{
    public enum FarmTaskStatus
    {
        Pending,
        InProgress,
        Completed,
        Delayed,
        Cancelled
    }
    
    public enum TaskPriority
    {
        Low,
        Medium,
        High,
        Urgent
    }
    
    public class TaskAssignment
    {
        public int Id { get; set; }
        
        [Required]
        public int EmployeeId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Title { get; set; }
        
        [StringLength(500)]
        public string Description { get; set; }
        
        [Required]
        [Display(Name = "Due Date")]
        public DateTime DueDate { get; set; }
        
        [Display(Name = "Creation Date")]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        
        [Display(Name = "Last Updated")]
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
        
        [Display(Name = "Completion Date")]
        public DateTime? CompletionDate { get; set; }
        
        [Required]
        public FarmTaskStatus Status { get; set; } = FarmTaskStatus.Pending;
        
        [Display(Name = "Is Completed")]
        public bool IsCompleted => Status == FarmTaskStatus.Completed;
        
        [Required]
        public TaskPriority Priority { get; set; } = TaskPriority.Medium;
        
        [StringLength(500)]
        public string Notes { get; set; }
        
        // Task category
        [Required]
        public int TaskCategoryId { get; set; }
        
        // Related animal ID if applicable
        public int? AnimalId { get; set; }
        
        // Navigation properties
        [ForeignKey("EmployeeId")]
        public virtual Employee Employee { get; set; }
        
        [ForeignKey("TaskCategoryId")]
        public virtual TaskCategory Category { get; set; }
        
        // Optional relation to an animal
        [ForeignKey("AnimalId")]
        public virtual Animal Animal { get; set; }
    }
} 