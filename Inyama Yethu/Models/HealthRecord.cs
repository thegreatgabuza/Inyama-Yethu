using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inyama_Yethu.Models
{
    public enum HealthRecordType
    {
        Vaccination,
        Medication,
        Illness,
        Injury,
        RoutineCheck,
        Other
    }
    
    public class HealthRecord
    {
        public int Id { get; set; }
        
        [Required]
        public int AnimalId { get; set; }
        
        [Required]
        [Display(Name = "Record Date")]
        public DateTime RecordDate { get; set; }
        
        [Required]
        [Display(Name = "Record Type")]
        public HealthRecordType RecordType { get; set; }
        
        // Treatment Type for views
        [Required]
        [StringLength(100)]
        [Display(Name = "Treatment Type")]
        public string TreatmentType { get; set; }
        
        [Required]
        [StringLength(100)]
        [Display(Name = "Treatment")]
        public string Treatment { get; set; }
        
        [StringLength(100)]
        [Display(Name = "Administered By")]
        public string AdministeredBy { get; set; }
        
        [Display(Name = "Cost (ZAR)")]
        public decimal? Cost { get; set; }
        
        [Display(Name = "Follow-up Date")]
        public DateTime? FollowUpDate { get; set; }
        
        [Display(Name = "Follow-up Completed")]
        public bool FollowUpCompleted { get; set; } = false;
        
        [StringLength(500)]
        [Display(Name = "Description")]
        public string Description { get; set; }
        
        [StringLength(500)]
        public string Notes { get; set; }
        
        // Additional properties needed for views
        [StringLength(100)]
        [Display(Name = "Medication")]
        public string Medication { get; set; }
        
        [StringLength(50)]
        [Display(Name = "Dosage")]
        public string Dosage { get; set; }
        
        [StringLength(100)]
        [Display(Name = "Treatment Outcome")]
        public string TreatmentOutcome { get; set; }
        
        // Record keeping fields
        public int? PerformedById { get; set; }
        
        [ForeignKey("PerformedById")]
        public virtual Employee PerformedBy { get; set; }
        
        // Navigation property
        [ForeignKey("AnimalId")]
        public virtual Animal Animal { get; set; }
    }
} 