using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inyama_Yethu.Models
{
    public class Feedback
    {
        public int Id { get; set; }
        
        [Required]
        public int CustomerId { get; set; }
        
        [Required]
        [Display(Name = "Feedback Date")]
        public DateTime FeedbackDate { get; set; } = DateTime.Now;
        
        [Required]
        [Range(1, 5)]
        [Display(Name = "Rating (1-5)")]
        public int Rating { get; set; }
        
        [StringLength(500)]
        [Display(Name = "Comments")]
        public string Comments { get; set; }
        
        [Display(Name = "Product Quality Rating")]
        [Range(1, 5)]
        public int? ProductQualityRating { get; set; }
        
        [Display(Name = "Delivery Rating")]
        [Range(1, 5)]
        public int? DeliveryRating { get; set; }
        
        [Display(Name = "Price Rating")]
        [Range(1, 5)]
        public int? PriceRating { get; set; }
        
        [Display(Name = "Service Rating")]
        [Range(1, 5)]
        public int? ServiceRating { get; set; }
        
        [Display(Name = "Has Been Addressed")]
        public bool HasBeenAddressed { get; set; } = false;
        
        [Display(Name = "Response")]
        [StringLength(500)]
        public string Response { get; set; }
        
        [Display(Name = "Response Date")]
        public DateTime? ResponseDate { get; set; }
        
        // Navigation property
        [ForeignKey("CustomerId")]
        public virtual Customer Customer { get; set; }
    }
} 