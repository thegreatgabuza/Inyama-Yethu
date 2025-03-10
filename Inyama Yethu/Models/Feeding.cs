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
        
        [Required]
        [Display(Name = "Feed Type")]
        public FeedType FeedType { get; set; }
        
        [Required]
        [Display(Name = "Quantity (kg)")]
        public double Quantity { get; set; }
        
        [Display(Name = "Cost per kg (ZAR)")]
        public decimal CostPerKg { get; set; }
        
        [NotMapped]
        [Display(Name = "Total Cost (ZAR)")]
        public decimal TotalCost => CostPerKg * (decimal)Quantity;
        
        [StringLength(500)]
        public string Notes { get; set; }
        
        // Navigation property
        [ForeignKey("AnimalId")]
        public virtual Animal Animal { get; set; }
    }
} 