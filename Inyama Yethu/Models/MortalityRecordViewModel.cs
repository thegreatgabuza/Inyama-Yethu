using System;
using System.ComponentModel.DataAnnotations;

namespace Inyama_Yethu.Models
{
    public class MortalityRecordViewModel
    {
        [Required]
        public int AnimalId { get; set; }
        
        [Required]
        [Display(Name = "Date of Death")]
        public DateTime RecordDate { get; set; }
        
        [Required]
        [Display(Name = "Cause of Death")]
        public string CauseOfDeath { get; set; }
        
        [Display(Name = "Additional Notes")]
        public string Notes { get; set; }
    }
} 