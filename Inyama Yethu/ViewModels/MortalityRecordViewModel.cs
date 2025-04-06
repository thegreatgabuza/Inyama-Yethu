using System;
using System.ComponentModel.DataAnnotations;

namespace Inyama_Yethu.ViewModels
{
    public class MortalityRecordViewModel
    {
        [Required(ErrorMessage = "Animal is required")]
        [Display(Name = "Animal")]
        public int AnimalId { get; set; }
        
        [Required(ErrorMessage = "Date of death is required")]
        [Display(Name = "Date of Death")]
        [DataType(DataType.Date)]
        public DateTime DeathDate { get; set; } = DateTime.Today;
        
        [Required(ErrorMessage = "Cause of death is required")]
        [Display(Name = "Cause of Death")]
        public string Cause { get; set; }
        
        [Display(Name = "Weight at Death (kg)")]
        [Range(0.1, 1000, ErrorMessage = "Weight must be between 0.1 and 1000 kg")]
        public double? Weight { get; set; }
        
        [Display(Name = "Additional Notes")]
        [DataType(DataType.MultilineText)]
        [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
        public string Notes { get; set; }
    }
} 