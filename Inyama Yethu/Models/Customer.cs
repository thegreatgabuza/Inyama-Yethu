using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Inyama_Yethu.Models
{
    public class Customer
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        [Display(Name = "Business Name")]
        public string BusinessName { get; set; }
        
        [Required]
        [StringLength(100)]
        [Display(Name = "Contact Person")]
        public string ContactPerson { get; set; }
        
        [Required]
        [StringLength(100)]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }
        
        [Required]
        [StringLength(20)]
        [Display(Name = "Phone")]
        public string Phone { get; set; }
        
        [StringLength(200)]
        [Display(Name = "Address")]
        public string Address { get; set; }
        
        [StringLength(50)]
        [Display(Name = "Township")]
        public string Township { get; set; }
        
        [Display(Name = "Registration Date")]
        public DateTime RegistrationDate { get; set; } = DateTime.Now;
        
        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;
        
        [StringLength(500)]
        [Display(Name = "Notes")]
        public string Notes { get; set; }
        
        // Navigation properties
        public virtual ICollection<Order> Orders { get; set; }
        public virtual ICollection<Feedback> Feedbacks { get; set; }
        
        public Customer()
        {
            Orders = new HashSet<Order>();
            Feedbacks = new HashSet<Feedback>();
        }
    }
} 