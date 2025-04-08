using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Inyama_Yethu.Models
{
    public enum ActivityType
    {
        Create,
        Read,
        Update,
        Delete
    }

    public class ActivityLog
    {
        public int Id { get; set; }
        
        [Required]
        public ActivityType ActivityType { get; set; }
        
        [Required]
        [StringLength(100)]
        public string EntityName { get; set; }
        
        public int EntityId { get; set; }
        
        [Required]
        [StringLength(500)]
        public string Description { get; set; }
        
        [Required]
        [StringLength(450)]
        public string UserId { get; set; }
        
        [ForeignKey("UserId")]
        public virtual IdentityUser User { get; set; }
        
        [StringLength(100)]
        public string UserName { get; set; }
        
        [Required]
        public DateTime Timestamp { get; set; } = DateTime.Now;
        
        // Additional fields for storing old and new values (JSON format)
        public string OldValues { get; set; }
        
        public string NewValues { get; set; }
        
        // For employee actions, store the employee ID
        public int? EmployeeId { get; set; }
        
        [ForeignKey("EmployeeId")]
        public virtual Employee Employee { get; set; }
    }
} 