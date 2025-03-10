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
        
        [Display(Name = "Completion Date")]
        public DateTime? CompletionDate { get; set; }
        
        [Required]
        public FarmTaskStatus Status { get; set; } = FarmTaskStatus.Pending;
        
        [Required]
        public TaskPriority Priority { get; set; } = TaskPriority.Medium;
        
        [StringLength(500)]
        public string Notes { get; set; }
        
        // Task category (e.g., feeding, vaccination, processing)
        [StringLength(50)]
        public string Category { get; set; }
        
        // Related animal ID if applicable
        public int? AnimalId { get; set; }
        
        // Navigation properties
        [ForeignKey("EmployeeId")]
        public virtual Employee Employee { get; set; }
        
        // Optional relation to an animal
        [ForeignKey("AnimalId")]
        public virtual Animal Animal { get; set; }
    }
} 