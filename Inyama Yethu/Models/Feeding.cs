using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inyama_Yethu.Models
{
    public enum FeedType
    {
        CreepPellets,
        WeanerFeed,
        GrowerFeed,
        SowFeed,
        BoarFeed,
        Other
    }
    
    public class Feeding
    {
        public int Id { get; set; }
        
        [Required]
        public int AnimalId { get; set; }
        
        [Required]
        [Display(Name = "Feed Date")]
        public DateTime FeedDate { get; set; }
        
        // Property for views using string FeedType
        [Required]
        [StringLength(100)]
        [Display(Name = "Feed Type")]
        public string FeedTypeString { get; set; }
        
        [Required]
        [Display(Name = "Feed Type")]
        public FeedType FeedType { get; set; }
        
        [Required]
        [Display(Name = "Quantity (kg)")]
        public double Quantity { get; set; }
        
        // Alias property for view compatibility
        [Display(Name = "Amount (kg)")]
        public double Amount => Quantity;
        
        [Display(Name = "Cost per kg (ZAR)")]
        public decimal? CostPerKg { get; set; }
        
        [NotMapped]
        [Display(Name = "Total Cost (ZAR)")]
        public decimal? TotalCost => CostPerKg.HasValue ? CostPerKg.Value * (decimal)Quantity : null;
        
        [StringLength(500)]
        public string Notes { get; set; }
        
        // Date field for view compatibility
        [Display(Name = "Feeding Date")]
        public DateTime FeedingDate { get => FeedDate; set => FeedDate = value; }
        
        // Record keeping fields
        public int? RecordedById { get; set; }
        public DateTime? RecordedDate { get; set; }
        
        [ForeignKey("RecordedById")]
        public virtual Employee RecordedBy { get; set; }
        
        // Navigation property
        [ForeignKey("AnimalId")]
        public virtual Animal Animal { get; set; }
    }
} 