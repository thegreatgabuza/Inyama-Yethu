using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inyama_Yethu.Models
{
    public class PigletProcessing
    {
        public int Id { get; set; }
        
        [Required]
        public int AnimalId { get; set; }
        
        [Display(Name = "Tail Docking")]
        public bool TailDockingCompleted { get; set; } = false;
        
        [Display(Name = "Tail Docking Date")]
        public DateTime? TailDockingDate { get; set; }
        
        [Display(Name = "Iron Injection")]
        public bool IronInjectionCompleted { get; set; } = false;
        
        [Display(Name = "Iron Injection Date")]
        public DateTime? IronInjectionDate { get; set; }
        
        [Display(Name = "Ear Notching")]
        public bool EarNotchingCompleted { get; set; } = false;
        
        [Display(Name = "Ear Notching Date")]
        public DateTime? EarNotchingDate { get; set; }
        
        [Display(Name = "Tattooing")]
        public bool TattooingCompleted { get; set; } = false;
        
        [Display(Name = "Tattooing Date")]
        public DateTime? TattooingDate { get; set; }
        
        [Display(Name = "Initial Vaccination")]
        public bool InitialVaccinationCompleted { get; set; } = false;
        
        [Display(Name = "Initial Vaccination Date")]
        public DateTime? InitialVaccinationDate { get; set; }
        
        [Display(Name = "Creep Feed Introduction")]
        public bool CreepFeedIntroductionCompleted { get; set; } = false;
        
        [Display(Name = "Creep Feed Introduction Date")]
        public DateTime? CreepFeedIntroductionDate { get; set; }
        
        [Display(Name = "Weighing at 21 Days")]
        public bool Weighing21DaysCompleted { get; set; } = false;
        
        [Display(Name = "Weight at 21 Days (kg)")]
        public double? WeightAt21Days { get; set; }
        
        [Display(Name = "Weighing at 28 Days")]
        public bool Weighing28DaysCompleted { get; set; } = false;
        
        [Display(Name = "Weight at 28 Days (kg)")]
        public double? WeightAt28Days { get; set; }
        
        [Display(Name = "Weaning Completed")]
        public bool WeaningCompleted { get; set; } = false;
        
        [Display(Name = "Weaning Date")]
        public DateTime? WeaningDate { get; set; }
        
        [StringLength(500)]
        public string Notes { get; set; }
        
        // Navigation property
        [ForeignKey("AnimalId")]
        public virtual Animal Animal { get; set; }
    }
} 