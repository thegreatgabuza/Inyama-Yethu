using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inyama_Yethu.Models
{
    public class Attendance
    {
        public int Id { get; set; }
        
        [Required]
        public int EmployeeId { get; set; }
        
        [Required]
        [Display(Name = "Check-In Time")]
        public DateTime CheckInTime { get; set; }
        
        [Display(Name = "Check-Out Time")]
        public DateTime? CheckOutTime { get; set; }
        
        public string Notes { get; set; }
        
        [NotMapped]
        public TimeSpan? Duration => CheckOutTime.HasValue ? CheckOutTime.Value - CheckInTime : null;
        
        // Navigation property
        [ForeignKey("EmployeeId")]
        public virtual Employee Employee { get; set; }
    }
} 