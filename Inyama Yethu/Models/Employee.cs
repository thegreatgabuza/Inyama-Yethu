using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Inyama_Yethu.Models
{
    public class Employee
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [StringLength(100)]
        [EmailAddress]
        public string Email { get; set; }

        [StringLength(20)]
        public string Phone { get; set; }
        
        [StringLength(100)]
        public string Position { get; set; }
        
        public DateTime HireDate { get; set; }
        
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual ICollection<Attendance> Attendances { get; set; }
        public virtual ICollection<TaskAssignment> TaskAssignments { get; set; }
        
        public Employee()
        {
            Attendances = new HashSet<Attendance>();
            TaskAssignments = new HashSet<TaskAssignment>();
        }
    }
} 