using System;
using System.ComponentModel.DataAnnotations;

namespace Inyama_Yethu.Models
{
    public class FeedInventory
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Feed Type")]
        public FeedType FeedType { get; set; }

        [Required]
        [Display(Name = "Current Stock (kg)")]
        public double CurrentStock { get; set; }

        [Required]
        [Display(Name = "Minimum Stock Level (kg)")]
        public double MinimumStockLevel { get; set; }

        [Required]
        [Display(Name = "Last Updated")]
        public DateTime LastUpdated { get; set; }

        [Display(Name = "Last Updated By")]
        public int? LastUpdatedById { get; set; }

        public virtual Employee LastUpdatedBy { get; set; }

        [StringLength(500)]
        [Display(Name = "Notes")]
        public string? Notes { get; set; }
    }
} 