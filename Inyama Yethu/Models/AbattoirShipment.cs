using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Inyama_Yethu.Models
{
    public enum ShipmentStatus
    {
        Scheduled,
        InTransit,
        Delivered,
        Processed,
        Cancelled
    }
    
    public class AbattoirShipment
    {
        public int Id { get; set; }
        
        [Required]
        [Display(Name = "Shipment Date")]
        public DateTime ShipmentDate { get; set; }
        
        [Required]
        [StringLength(100)]
        [Display(Name = "Abattoir Name")]
        public string AbattoirName { get; set; }
        
        [Required]
        [Display(Name = "Number of Pigs")]
        public int NumberOfPigs { get; set; }
        
        [Required]
        [Display(Name = "Status")]
        public ShipmentStatus Status { get; set; } = ShipmentStatus.Scheduled;
        
        [Display(Name = "Transportation Cost (ZAR)")]
        public decimal TransportationCost { get; set; }
        
        [Display(Name = "Total Weight (kg)")]
        public double? TotalWeight { get; set; }
        
        [Display(Name = "Estimated Value (ZAR)")]
        public decimal EstimatedValue { get; set; }
        
        [Display(Name = "Actual Payment (ZAR)")]
        public decimal? ActualPayment { get; set; }
        
        [Display(Name = "Payment Date")]
        public DateTime? PaymentDate { get; set; }
        
        [StringLength(500)]
        public string Notes { get; set; }
        
        // Navigation property for the animals in this shipment
        public virtual ICollection<Animal> Animals { get; set; }
        
        public AbattoirShipment()
        {
            Animals = new HashSet<Animal>();
        }
    }
} 