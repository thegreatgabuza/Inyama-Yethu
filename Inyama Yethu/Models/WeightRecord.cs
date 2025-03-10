using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inyama_Yethu.Models
{
    public class WeightRecord
    {
        public int Id { get; set; }
        
        [Required]
        public int AnimalId { get; set; }
        
        [Required]
        [Display(Name = "Record Date")]
        public DateTime RecordDate { get; set; }
        
        [Required]
        [Display(Name = "Weight (kg)")]
        public double Weight { get; set; }
        
        [StringLength(500)]
        public string Notes { get; set; }
        
        // Navigation property
        [ForeignKey("AnimalId")]
        public virtual Animal Animal { get; set; }
        
        // Not mapped properties for weight analysis
        [NotMapped]
        public double? PreviousWeight { get; private set; }
        
        [NotMapped]
        [Display(Name = "Weight Gain (kg)")]
        public double? WeightGain => PreviousWeight.HasValue ? Weight - PreviousWeight.Value : null;
        
        // Method to set previous weight from repository data
        public void SetPreviousWeight(double? value)
        {
            PreviousWeight = value;
        }
    }
} 